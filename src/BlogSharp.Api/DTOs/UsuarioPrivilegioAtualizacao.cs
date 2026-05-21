using System.ComponentModel.DataAnnotations;

namespace BlogSharp.Api.DTOs;

public class UsuarioPrivilegioAtualizacao
{
    /// <summary>
    /// Novo perfil do usuario. Valores aceitos: Usuario ou Admin.
    /// </summary>
    [Required]
    [StringLength(30)]
    public string Tipo { get; set; } = string.Empty;
}
