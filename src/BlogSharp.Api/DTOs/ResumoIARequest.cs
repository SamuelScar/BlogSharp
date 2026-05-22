using System.ComponentModel.DataAnnotations;

namespace BlogSharp.Api.DTOs;

public class ResumoIARequest
{
    /// <summary>
    /// Conteudo da postagem que sera analisado pela IA.
    /// </summary>
    [Required]
    [StringLength(5000)]
    public string Conteudo { get; set; } = string.Empty;
}
