
using Microsoft.AspNetCore.Mvc;
using AcademiaAPI.Models;
using AcademiaAPI.Services;

namespace AcademiaAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AcademiaController : ControllerBase
{
    private readonly JsonService _jsonService;

    public AcademiaController(JsonService jsonService)
    {
        _jsonService = jsonService;
    }

    private const string Arquivo = "academias.json";

    // GET: api/academia
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var academias = await _jsonService.ReadAsync<Academia>(Arquivo);

        return Ok(academias);
    }

    // GET: api/academia/1
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var academias = await _jsonService.ReadAsync<Academia>(Arquivo);

        var academia = academias.FirstOrDefault(a => a.IdAcademia == id);

        if (academia == null)
            return NotFound("Academia não encontrada.");

        return Ok(academia);
    }

    // POST: api/academia
    [HttpPost]
    public async Task<IActionResult> Create(Academia academia)
    {
        var academias = await _jsonService.ReadAsync<Academia>(Arquivo);

        if (string.IsNullOrWhiteSpace(academia.Nome))
            return BadRequest("O nome da academia é obrigatório.");

        if (string.IsNullOrWhiteSpace(academia.Endereco))
            return BadRequest("O endereço da academia é obrigatório.");

        if (academia.CapacidadeMaxima <= 0)
            return BadRequest("A capacidade máxima deve ser maior que zero.");

        if (academias.Any(a =>
            a.Nome.Equals(academia.Nome.Trim(),
                StringComparison.OrdinalIgnoreCase)))
        {
            return Conflict("Já existe uma academia com esse nome.");
        }

        academia.IdAcademia = academias.Count == 0
            ? 1
            : academias.Max(a => a.IdAcademia) + 1;

        academia.Nome = academia.Nome.Trim();
        academia.Endereco = academia.Endereco.Trim();

        academias.Add(academia);

        await _jsonService.WriteAsync(Arquivo, academias);

        return CreatedAtAction(
            nameof(GetById),
            new { id = academia.IdAcademia },
            academia
        );
    }

    // PUT: api/academia/1
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, Academia academiaAtualizada)
    {
        var academias = await _jsonService.ReadAsync<Academia>(Arquivo);

        var academia = academias.FirstOrDefault(a => a.IdAcademia == id);

        if (academia == null)
            return NotFound("Academia não encontrada.");

        if (string.IsNullOrWhiteSpace(academiaAtualizada.Nome))
            return BadRequest("O nome da academia é obrigatório.");

        if (string.IsNullOrWhiteSpace(academiaAtualizada.Endereco))
            return BadRequest("O endereço da academia é obrigatório.");

        if (academiaAtualizada.CapacidadeMaxima <= 0)
            return BadRequest("A capacidade máxima deve ser maior que zero.");

        var nomeDuplicado = academias.Any(a =>
            a.IdAcademia != id &&
            a.Nome.Equals(academiaAtualizada.Nome.Trim(),
                StringComparison.OrdinalIgnoreCase));

        if (nomeDuplicado)
            return Conflict("Já existe outra academia com esse nome.");

        var alunos = await _jsonService.ReadAsync<Aluno>("alunos.json");

        int quantidadeAlunos = alunos.Count(a => a.AcademiaId == id);

        if (academiaAtualizada.CapacidadeMaxima < quantidadeAlunos)
            return BadRequest("A capacidade não pode ser menor que a quantidade atual de alunos.");

        academia.Nome = academiaAtualizada.Nome.Trim();
        academia.Endereco = academiaAtualizada.Endereco.Trim();
        academia.CapacidadeMaxima = academiaAtualizada.CapacidadeMaxima;

        await _jsonService.WriteAsync(Arquivo, academias);

        return Ok(academia);
    }

    // DELETE: api/academia/1
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var academias = await _jsonService.ReadAsync<Academia>(Arquivo);

        var academia = academias.FirstOrDefault(a => a.IdAcademia == id);

        if (academia == null)
            return NotFound("Academia não encontrada.");

        var alunos = await _jsonService.ReadAsync<Aluno>("alunos.json");
        var instrutores = await _jsonService.ReadAsync<Instrutor>("instrutores.json");

        bool possuiAlunos = alunos.Any(a => a.AcademiaId == id);
        bool possuiInstrutores = instrutores.Any(i => i.AcademiaId == id);

        if (possuiAlunos || possuiInstrutores)
            return Conflict("Não é possível excluir uma academia que possui alunos ou instrutores cadastrados.");

        academias.Remove(academia);

        await _jsonService.WriteAsync(Arquivo, academias);

        return Ok("Academia excluída com sucesso.");
    }
}
