using System.ComponentModel.DataAnnotations;

namespace AcademiaAPI.Models;

public class Aluno
{
    [Key]
    public int IdAluno { get; set; }

    public string Nome { get; set; } = string.Empty;

    public string Cpf { get; set; } = string.Empty;

    public string Telefone { get; set; } = string.Empty;

    public bool MatriculaAtiva { get; set; }

    public int AcademiaId { get; set; }

    public Academia? Academia { get; set; }

    public ICollection<Treino> Treinos { get; set; } = new List<Treino>();
}