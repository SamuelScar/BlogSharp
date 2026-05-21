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

    /// <summary>
    /// Nova senha do usuario. Se nao for informada, a senha atual sera mantida.
    /// </summary>
    [StringLength(100, MinimumLength = 6)]
    public string? Senha { get; set; }

    [StringLength(500)]
    public string? Foto { get; set; }
}
