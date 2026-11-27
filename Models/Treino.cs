using System.ComponentModel.DataAnnotations;

namespace AcademiaAPI.Models;

public class Treino
{
    [Key]
    public int IdTreino { get; set; }

    public string Objetivo { get; set; } = string.Empty;

    public DateTime DataCriacao { get; set; }

    public string Descricao { get; set; } = string.Empty;

    public bool Ativo { get; set; }

    public int InstrutorId { get; set; }

    public Instrutor? Instrutor { get; set; }

    public int AlunoId { get; set; }

    public Aluno? Aluno { get; set; }
}