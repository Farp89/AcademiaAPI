using System.ComponentModel.DataAnnotations;

namespace AcademiaAPI.Models;

public class Academia
{
    [Key]
    public int IdAcademia { get; set; }

    public string Nome { get; set; } = string.Empty;

    public string Endereco { get; set; } = string.Empty;

    public int CapacidadeMaxima { get; set; }

    public ICollection<Aluno> Alunos { get; set; } = new List<Aluno>();

    public ICollection<Instrutor> Instrutores { get; set; } = new List<Instrutor>();
}