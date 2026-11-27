
using Microsoft.AspNetCore.Mvc;
using AcademiaAPI.Models;
using AcademiaAPI.Services;

namespace AcademiaAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TreinoController : ControllerBase
{
    private readonly JsonService _jsonService;

    private const string Arquivo = "treinos.json";
    private const string ArquivoAlunos = "alunos.json";
    private const string ArquivoInstrutores = "instrutores.json";

    public TreinoController(JsonService jsonService)
    {
        _jsonService = jsonService;
    }

    // GET: api/treino
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var treinos = await _jsonService.ReadAsync<Treino>(Arquivo);
        return Ok(treinos);
    }

    // GET: api/treino/1
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var treinos = await _jsonService.ReadAsync<Treino>(Arquivo);

        var treino = treinos.FirstOrDefault(t => t.IdTreino == id);

        if (treino == null)
            return NotFound("Treino não encontrado.");

        return Ok(treino);
    }

    // POST: api/treino
    [HttpPost]
    public async Task<IActionResult> Create(Treino treino)
    {
        var treinos = await _jsonService.ReadAsync<Treino>(Arquivo);
        var instrutores = await _jsonService.ReadAsync<Instrutor>(ArquivoInstrutores);
        var alunos = await _jsonService.ReadAsync<Aluno>(ArquivoAlunos);

        if (string.IsNullOrWhiteSpace(treino.Objetivo))
            return BadRequest("O objetivo do treino é obrigatório.");

        if (string.IsNullOrWhiteSpace(treino.Descricao))
            return BadRequest("A descrição do treino é obrigatória.");

        var instrutorExiste = instrutores.Any(i =>
            i.IdInstrutor == treino.InstrutorId);

        if (!instrutorExiste)
            return BadRequest("O instrutor informado não existe.");

        var aluno = alunos.FirstOrDefault(a => a.IdAluno == treino.AlunoId);

        if (aluno == null)
            return BadRequest("O aluno informado não existe.");

        if (!aluno.MatriculaAtiva)
            return BadRequest("Não é possível vincular um treino a um aluno com matrícula inativa.");

        treino.IdTreino = treinos.Count > 0
            ? treinos.Max(t => t.IdTreino) + 1
            : 1;

        if (treino.DataCriacao == default)
            treino.DataCriacao = DateTime.UtcNow;

        treino.Objetivo = treino.Objetivo.Trim();
        treino.Descricao = treino.Descricao.Trim();

        // Não armazenar objetos de navegação no JSON.
        treino.Instrutor = null;
        treino.Aluno = null;

        treinos.Add(treino);

        await _jsonService.WriteAsync(Arquivo, treinos);

        return CreatedAtAction(
            nameof(GetById),
            new { id = treino.IdTreino },
            treino
        );
    }

    // PUT: api/treino/1
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, Treino dados)
    {
        var treinos = await _jsonService.ReadAsync<Treino>(Arquivo);
        var instrutores = await _jsonService.ReadAsync<Instrutor>(ArquivoInstrutores);
        var alunos = await _jsonService.ReadAsync<Aluno>(ArquivoAlunos);

        var treino = treinos.FirstOrDefault(t => t.IdTreino == id);

        if (treino == null)
            return NotFound("Treino não encontrado.");

        if (string.IsNullOrWhiteSpace(dados.Objetivo))
            return BadRequest("O objetivo do treino é obrigatório.");

        if (string.IsNullOrWhiteSpace(dados.Descricao))
            return BadRequest("A descrição do treino é obrigatória.");

        var instrutorExiste = instrutores.Any(i =>
            i.IdInstrutor == dados.InstrutorId);

        if (!instrutorExiste)
            return BadRequest("O instrutor informado não existe.");

        var aluno = alunos.FirstOrDefault(a => a.IdAluno == dados.AlunoId);

        if (aluno == null)
            return BadRequest("O aluno informado não existe.");

        if (!aluno.MatriculaAtiva)
            return BadRequest("Não é possível vincular um treino a um aluno com matrícula inativa.");

        treino.Objetivo = dados.Objetivo.Trim();
        treino.Descricao = dados.Descricao.Trim();
        treino.InstrutorId = dados.InstrutorId;
        treino.AlunoId = dados.AlunoId;
        treino.Ativo = dados.Ativo;

        if (dados.DataCriacao != default)
            treino.DataCriacao = dados.DataCriacao;

        treino.Instrutor = null;
        treino.Aluno = null;

        await _jsonService.WriteAsync(Arquivo, treinos);

        return Ok(treino);
    }

    // DELETE: api/treino/1
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var treinos = await _jsonService.ReadAsync<Treino>(Arquivo);

        var treino = treinos.FirstOrDefault(t => t.IdTreino == id);

        if (treino == null)
            return NotFound("Treino não encontrado.");

        treinos.Remove(treino);

        await _jsonService.WriteAsync(Arquivo, treinos);

        return Ok("Treino excluído com sucesso.");
    }

    // POST: api/treino/1/alunos/2
    // Vincula um aluno a um treino.
    [HttpPost("{id:int}/alunos/{alunoId:int}")]
    public async Task<IActionResult> VincularAluno(int id, int alunoId)
    {
        var treinos = await _jsonService.ReadAsync<Treino>(Arquivo);
        var alunos = await _jsonService.ReadAsync<Aluno>(ArquivoAlunos);

        var treino = treinos.FirstOrDefault(t => t.IdTreino == id);

        if (treino == null)
            return NotFound("Treino não encontrado.");

        var aluno = alunos.FirstOrDefault(a => a.IdAluno == alunoId);

        if (aluno == null)
            return NotFound("Aluno não encontrado.");

        if (!aluno.MatriculaAtiva)
            return BadRequest("Não é possível vincular um aluno com matrícula inativa.");

        if (treino.AlunoId == alunoId)
            return Conflict("Este aluno já está vinculado a este treino.");

        treino.AlunoId = alunoId;
        treino.Aluno = null;

        await _jsonService.WriteAsync(Arquivo, treinos);

        return Ok("Aluno vinculado ao treino com sucesso.");
    }

    // DELETE: api/treino/1/alunos/2
    // Desvincula um aluno de um treino.
    [HttpDelete("{id:int}/alunos/{alunoId:int}")]
    public async Task<IActionResult> DesvincularAluno(int id, int alunoId)
    {
        var treinos = await _jsonService.ReadAsync<Treino>(Arquivo);
        var treino = treinos.FirstOrDefault(t => t.IdTreino == id);

        if (treino == null)
            return NotFound("Treino não encontrado.");

        if (treino.AlunoId != alunoId)
            return NotFound("Vínculo entre aluno e treino não encontrado.");

        treino.AlunoId = 0;
        treino.Aluno = null;

        await _jsonService.WriteAsync(Arquivo, treinos);
        return Ok("Aluno desvinculado do treino com sucesso.");
    }

    // GET: api/treino/1/alunos
    // Lista os alunos vinculados a um treino.
    [HttpGet("{id:int}/alunos")]
    public async Task<IActionResult> GetAlunosDoTreino(int id)
    {
        var treinos = await _jsonService.ReadAsync<Treino>(Arquivo);
        var alunos = await _jsonService.ReadAsync<Aluno>(ArquivoAlunos);

        var treino = treinos.FirstOrDefault(t => t.IdTreino == id);

        if (treino == null)
            return NotFound("Treino não encontrado.");

        var alunoDoTreino = alunos
            .Where(a => a.IdAluno == treino.AlunoId)
            .ToList();

        return Ok(alunoDoTreino);
    }

    // GET: api/treino/aluno/1
    // Lista os treinos vinculados a um aluno.
    [HttpGet("aluno/{alunoId:int}")]
    public async Task<IActionResult> GetTreinosDoAluno(int alunoId)
    {
        var alunos = await _jsonService.ReadAsync<Aluno>(ArquivoAlunos);
        var treinos = await _jsonService.ReadAsync<Treino>(Arquivo);

        var alunoExiste = alunos.Any(a => a.IdAluno == alunoId);

        if (!alunoExiste)
            return NotFound("Aluno não encontrado.");

        var treinosDoAluno = treinos
            .Where(t => t.AlunoId == alunoId)
            .ToList();

        return Ok(treinosDoAluno);
    }
}
