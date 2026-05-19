using System.ComponentModel.DataAnnotations;

namespace BlogSharp.Api.DTOs;

public class TemaAtualizacao
{
    [Required]
    [StringLength(100)]
    public string Descricao { get; set; } = string.Empty;
}
