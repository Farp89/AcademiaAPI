
using Microsoft.AspNetCore.Mvc;
using AcademiaAPI.Models;
using AcademiaAPI.Services;

namespace AcademiaAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AlunoController : ControllerBase
{
    private readonly JsonService _jsonService;

    private const string Arquivo = "alunos.json";

    public AlunoController(JsonService jsonService)
    {
        _jsonService = jsonService;
    }

    // GET: api/aluno
    // Lista todos os alunos
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var alunos = await _jsonService.ReadAsync<Aluno>(Arquivo);

        return Ok(alunos);
    }

    // GET: api/aluno/1
    // Busca um aluno pelo ID
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var alunos = await _jsonService.ReadAsync<Aluno>(Arquivo);

        var aluno = alunos.FirstOrDefault(a => a.IdAluno == id);

        if (aluno == null)
            return NotFound("Aluno não encontrado.");

        return Ok(aluno);
    }

    // POST: api/aluno
    // Cadastra um novo aluno
    [HttpPost]
    public async Task<IActionResult> Create(Aluno aluno)
    {
        var alunos = await _jsonService.ReadAsync<Aluno>(Arquivo);

        var academias = await _jsonService.ReadAsync<Academia>("academias.json");

        // Validação do nome
        if (string.IsNullOrWhiteSpace(aluno.Nome))
            return BadRequest("O nome do aluno é obrigatório.");

        // Validação do CPF
        if (string.IsNullOrWhiteSpace(aluno.Cpf) ||
            aluno.Cpf.Length != 11 ||
            !aluno.Cpf.All(char.IsDigit))
        {
            return BadRequest("O CPF deve conter exatamente 11 números.");
        }

        // Verifica CPF duplicado
        if (alunos.Any(a => a.Cpf == aluno.Cpf))
            return Conflict("Já existe um aluno cadastrado com esse CPF.");

        // Validação do telefone
        if (string.IsNullOrWhiteSpace(aluno.Telefone))
            return BadRequest("O telefone é obrigatório.");

        // Verifica se a academia existe
        var academia = academias.FirstOrDefault(
            a => a.IdAcademia == aluno.AcademiaId
        );

        if (academia == null)
            return BadRequest("A academia informada não existe.");

        // Verifica capacidade da academia
        var quantidadeAlunos = alunos.Count(
            a => a.AcademiaId == aluno.AcademiaId
        );

        if (quantidadeAlunos >= academia.CapacidadeMaxima)
            return Conflict("A academia atingiu sua capacidade máxima de alunos.");

        // Gera ID automaticamente
        aluno.IdAluno = alunos.Count == 0
            ? 1
            : alunos.Max(a => a.IdAluno) + 1;

        aluno.Nome = aluno.Nome.Trim();
        aluno.Cpf = aluno.Cpf.Trim();
        aluno.Telefone = aluno.Telefone.Trim();

        // Evita salvar objetos de navegação recebidos na requisição
        aluno.Academia = null;
        aluno.Treinos = new List<Treino>();

        // Nova matrícula começa ativa
        aluno.MatriculaAtiva = true;

        alunos.Add(aluno);

        await _jsonService.WriteAsync(Arquivo, alunos);

        return CreatedAtAction(
            nameof(GetById),
            new { id = aluno.IdAluno },
            aluno
        );
    }

    // PUT: api/aluno/1
    // Atualiza os dados de um aluno
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, Aluno alunoAtualizado)
    {
        var alunos = await _jsonService.ReadAsync<Aluno>(Arquivo);

        var aluno = alunos.FirstOrDefault(a => a.IdAluno == id);

        if (aluno == null)
            return NotFound("Aluno não encontrado.");

        if (string.IsNullOrWhiteSpace(alunoAtualizado.Nome))
            return BadRequest("O nome do aluno é obrigatório.");

        if (string.IsNullOrWhiteSpace(alunoAtualizado.Cpf) ||
            alunoAtualizado.Cpf.Length != 11 ||
            !alunoAtualizado.Cpf.All(char.IsDigit))
        {
            return BadRequest("O CPF deve conter exatamente 11 números.");
        }

        if (string.IsNullOrWhiteSpace(alunoAtualizado.Telefone))
            return BadRequest("O telefone é obrigatório.");

        // Verifica CPF duplicado em outro aluno
        bool cpfDuplicado = alunos.Any(a =>
            a.IdAluno != id &&
            a.Cpf == alunoAtualizado.Cpf.Trim()
        );

        if (cpfDuplicado)
            return Conflict("Esse CPF já pertence a outro aluno.");

        var academias = await _jsonService.ReadAsync<Academia>("academias.json");

        var academia = academias.FirstOrDefault(
            a => a.IdAcademia == alunoAtualizado.AcademiaId
        );

        if (academia == null)
            return BadRequest("A academia informada não existe.");

        // Verifica a capacidade caso o aluno seja transferido
        var quantidadeAlunos = alunos.Count(
            a => a.AcademiaId == alunoAtualizado.AcademiaId &&
                 a.IdAluno != id
        );

        if (aluno.AcademiaId != alunoAtualizado.AcademiaId &&
            quantidadeAlunos >= academia.CapacidadeMaxima)
        {
            return Conflict("A academia de destino atingiu sua capacidade máxima.");
        }

        aluno.Nome = alunoAtualizado.Nome.Trim();
        aluno.Cpf = alunoAtualizado.Cpf.Trim();
        aluno.Telefone = alunoAtualizado.Telefone.Trim();
        aluno.AcademiaId = alunoAtualizado.AcademiaId;

        await _jsonService.WriteAsync(Arquivo, alunos);

        return Ok(aluno);
    }

    // DELETE: api/aluno/1
    // Exclui um aluno
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var alunos = await _jsonService.ReadAsync<Aluno>(Arquivo);

        var aluno = alunos.FirstOrDefault(a => a.IdAluno == id);

        if (aluno == null)
            return NotFound("Aluno não encontrado.");

        var treinos = await _jsonService.ReadAsync<Treino>("treinos.json");
        bool possuiTreinos = treinos.Any(t => t.AlunoId == id);

        if (possuiTreinos)
        {
            return Conflict(
                "Não é possível excluir o aluno enquanto ele possuir treinos vinculados."
            );
        }

        alunos.Remove(aluno);

        await _jsonService.WriteAsync(Arquivo, alunos);

        return Ok("Aluno excluído com sucesso.");
    }

    // PATCH: api/aluno/1/ativar-matricula
    // Ativa a matrícula do aluno
    [HttpPatch("{id:int}/ativar-matricula")]
    public async Task<IActionResult> AtivarMatricula(int id)
    {
        var alunos = await _jsonService.ReadAsync<Aluno>(Arquivo);

        var aluno = alunos.FirstOrDefault(a => a.IdAluno == id);

        if (aluno == null)
            return NotFound("Aluno não encontrado.");

        if (aluno.MatriculaAtiva)
            return BadRequest("A matrícula já está ativa.");

        aluno.MatriculaAtiva = true;

        await _jsonService.WriteAsync(Arquivo, alunos);

        return Ok("Matrícula ativada com sucesso.");
    }

    // PATCH: api/aluno/1/cancelar-matricula
    // Cancela a matrícula do aluno
    [HttpPatch("{id:int}/cancelar-matricula")]
    public async Task<IActionResult> CancelarMatricula(int id)
    {
        var alunos = await _jsonService.ReadAsync<Aluno>(Arquivo);

        var aluno = alunos.FirstOrDefault(a => a.IdAluno == id);

        if (aluno == null)
            return NotFound("Aluno não encontrado.");

        if (!aluno.MatriculaAtiva)
            return BadRequest("A matrícula já está cancelada.");

        aluno.MatriculaAtiva = false;

        await _jsonService.WriteAsync(Arquivo, alunos);

        return Ok("Matrícula cancelada com sucesso.");
    }

    // GET: api/aluno/1/situacao
    // Verifica a situação da matrícula
    [HttpGet("{id:int}/situacao")]
    public async Task<IActionResult> VerificarSituacao(int id)
    {
        var alunos = await _jsonService.ReadAsync<Aluno>(Arquivo);

        var aluno = alunos.FirstOrDefault(a => a.IdAluno == id);

        if (aluno == null)
            return NotFound("Aluno não encontrado.");

        string situacao = aluno.MatriculaAtiva
            ? "Ativa"
            : "Cancelada";

        return Ok(new
        {
            aluno.IdAluno,
            aluno.Nome,
            Situacao = situacao
        });
    }
}
