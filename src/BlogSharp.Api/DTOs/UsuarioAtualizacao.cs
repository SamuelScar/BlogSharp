using System.ComponentModel.DataAnnotations;

namespace BlogSharp.Api.DTOs;

public class UsuarioAtualizacao
{
    [Required]
    [StringLength(100)]
    public string Nome { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(255)]
    public string Email { get; set; } = string.Empty;

    [StringLength(100, MinimumLength = 6)]
    public string? Senha { get; set; }

    [Required]
    [StringLength(30)]
    public string Tipo { get; set; } = "Usuario";

    [StringLength(500)]
    public string? Foto { get; set; }
}
