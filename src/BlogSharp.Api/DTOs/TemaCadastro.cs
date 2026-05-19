using System.ComponentModel.DataAnnotations;

namespace BlogSharp.Api.DTOs;

public class TemaCadastro
{
    [Required]
    [StringLength(100)]
    public string Descricao { get; set; } = string.Empty;
}
