
using Microsoft.AspNetCore.Mvc;
using AcademiaAPI.Models;
using AcademiaAPI.Services;

namespace AcademiaAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InstrutorController : ControllerBase
{
    private readonly JsonService _jsonService;

    private const string Arquivo = "instrutores.json";

    public InstrutorController(JsonService jsonService)
    {
        _jsonService = jsonService;
    }

    // GET: api/instrutor
    // Lista todos os instrutores
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var instrutores = await _jsonService.ReadAsync<Instrutor>(Arquivo);

        return Ok(instrutores);
    }

    // GET: api/instrutor/1
    // Busca instrutor pelo ID
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var instrutores = await _jsonService.ReadAsync<Instrutor>(Arquivo);

        var instrutor = instrutores.FirstOrDefault(
            i => i.IdInstrutor == id
        );

        if (instrutor == null)
            return NotFound("Instrutor não encontrado.");

        return Ok(instrutor);
    }

    // POST: api/instrutor
    // Cadastra um novo instrutor
    [HttpPost]
    public async Task<IActionResult> Create(Instrutor instrutor)
    {
        var instrutores = await _jsonService.ReadAsync<Instrutor>(Arquivo);

        var academias = await _jsonService.ReadAsync<Academia>(
            "academias.json"
        );

        if (string.IsNullOrWhiteSpace(instrutor.Nome))
            return BadRequest("O nome do instrutor é obrigatório.");

        if (string.IsNullOrWhiteSpace(instrutor.Cref))
            return BadRequest("O CREF do instrutor é obrigatório.");

        if (string.IsNullOrWhiteSpace(instrutor.Especialidade))
            return BadRequest("A especialidade do instrutor é obrigatória.");

        string cref = instrutor.Cref.Trim();

        // Verifica CREF duplicado
        if (instrutores.Any(i =>
            i.Cref.Equals(cref, StringComparison.OrdinalIgnoreCase)))
        {
            return Conflict("Já existe um instrutor cadastrado com esse CREF.");
        }

        // Verifica se a academia existe
        var academia = academias.FirstOrDefault(
            a => a.IdAcademia == instrutor.AcademiaId
        );

        if (academia == null)
            return BadRequest("A academia informada não existe.");

        // Gera ID automaticamente
        instrutor.IdInstrutor = instrutores.Count == 0
            ? 1
            : instrutores.Max(i => i.IdInstrutor) + 1;

        instrutor.Nome = instrutor.Nome.Trim();
        instrutor.Cref = cref;
        instrutor.Especialidade = instrutor.Especialidade.Trim();

        // Não recebe objetos de navegação do cliente
        instrutor.Academia = null;
        instrutor.Treinos = new List<Treino>();

        instrutores.Add(instrutor);

        await _jsonService.WriteAsync(Arquivo, instrutores);

        return CreatedAtAction(
            nameof(GetById),
            new { id = instrutor.IdInstrutor },
            instrutor
        );
    }

    // PUT: api/instrutor/1
    // Atualiza os dados de um instrutor
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        Instrutor instrutorAtualizado)
    {
        var instrutores = await _jsonService.ReadAsync<Instrutor>(Arquivo);

        var instrutor = instrutores.FirstOrDefault(
            i => i.IdInstrutor == id
        );

        if (instrutor == null)
            return NotFound("Instrutor não encontrado.");

        if (string.IsNullOrWhiteSpace(instrutorAtualizado.Nome))
            return BadRequest("O nome do instrutor é obrigatório.");

        if (string.IsNullOrWhiteSpace(instrutorAtualizado.Cref))
            return BadRequest("O CREF do instrutor é obrigatório.");

        if (string.IsNullOrWhiteSpace(instrutorAtualizado.Especialidade))
            return BadRequest("A especialidade do instrutor é obrigatória.");

        string cref = instrutorAtualizado.Cref.Trim();

        // Verifica se outro instrutor possui o mesmo CREF
        bool crefDuplicado = instrutores.Any(i =>
            i.IdInstrutor != id &&
            i.Cref.Equals(cref, StringComparison.OrdinalIgnoreCase)
        );

        if (crefDuplicado)
            return Conflict("Esse CREF já pertence a outro instrutor.");

        var academias = await _jsonService.ReadAsync<Academia>(
            "academias.json"
        );

        var academia = academias.FirstOrDefault(
            a => a.IdAcademia == instrutorAtualizado.AcademiaId
        );

        if (academia == null)
            return BadRequest("A academia informada não existe.");

        instrutor.Nome = instrutorAtualizado.Nome.Trim();
        instrutor.Cref = cref;
        instrutor.Especialidade = instrutorAtualizado.Especialidade.Trim();
        instrutor.AcademiaId = instrutorAtualizado.AcademiaId;

        await _jsonService.WriteAsync(Arquivo, instrutores);

        return Ok(instrutor);
    }

    // DELETE: api/instrutor/1
    // Exclui um instrutor
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var instrutores = await _jsonService.ReadAsync<Instrutor>(Arquivo);

        var instrutor = instrutores.FirstOrDefault(
            i => i.IdInstrutor == id
        );

        if (instrutor == null)
            return NotFound("Instrutor não encontrado.");

        var treinos = await _jsonService.ReadAsync<Treino>(
            "treinos.json"
        );

        // Verifica se existem treinos vinculados
        bool possuiTreinos = treinos.Any(
            t => t.InstrutorId == id
        );

        if (possuiTreinos)
        {
            return Conflict(
                "Não é possível excluir o instrutor enquanto possuir treinos cadastrados."
            );
        }

        instrutores.Remove(instrutor);

        await _jsonService.WriteAsync(Arquivo, instrutores);

        return Ok("Instrutor excluído com sucesso.");
    }
}
