using System.ComponentModel.DataAnnotations;

namespace BlogSharp.Api.DTOs;

public class ResumoIARequest
{
    [Required]
    [StringLength(5000)]
    public string Conteudo { get; set; } = string.Empty;
}
