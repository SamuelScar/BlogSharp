using System.ComponentModel.DataAnnotations;

namespace BlogSharp.Api.Models;

public class Tema
{
    public long Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Descricao { get; set; } = string.Empty;
}
