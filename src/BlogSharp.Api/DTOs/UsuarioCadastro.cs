using System.ComponentModel.DataAnnotations;

namespace BlogSharp.Api.DTOs;

public class UsuarioCadastro
{
    [Required]
    [StringLength(100)]
    public string Nome { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string Senha { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Foto { get; set; }
}
