using System.ComponentModel.DataAnnotations;

namespace BlogSharp.Api.Models;

public class Usuario
{
    public long Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Nome { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(255)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    public string SenhaHash { get; set; } = string.Empty;

    [Required]
    [StringLength(30)]
    public string Tipo { get; set; } = "Usuario";

    [StringLength(500)]
    public string? Foto { get; set; }
}
