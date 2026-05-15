using System.ComponentModel.DataAnnotations;

namespace BlogSharp.Api.DTOs;

public class UsuarioLogin
{
    [Required]
    [EmailAddress]
    [StringLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 6)]
    public string Senha { get; set; } = string.Empty;

    public string? Token { get; set; }

    public string? Nome { get; set; }

    public string? Tipo { get; set; }
}
