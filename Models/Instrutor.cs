using System.ComponentModel.DataAnnotations;

namespace AcademiaAPI.Models;

public class Instrutor
{
    [Key]
    public int IdInstrutor { get; set; }

    public string Nome { get; set; } = string.Empty;

    public string Cref { get; set; } = string.Empty;

    public string Especialidade { get; set; } = string.Empty;

    public int AcademiaId { get; set; }

    public Academia? Academia { get; set; }

    public ICollection<Treino> Treinos { get; set; } = new List<Treino>();
}