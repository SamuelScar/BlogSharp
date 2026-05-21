using System.ComponentModel.DataAnnotations;

namespace BlogSharp.Api.DTOs;

public class PostagemAtualizacao
{
    [Required]
    [StringLength(100)]
    public string Titulo { get; set; } = string.Empty;

    [Required]
    [StringLength(5000)]
    public string Conteudo { get; set; } = string.Empty;

    /// <summary>
    /// Identificador do usuario autor. Deve ser o mesmo usuario do token.
    /// </summary>
    [Range(1, long.MaxValue)]
    public long UsuarioId { get; set; }

    /// <summary>
    /// Identificador do tema vinculado a postagem.
    /// </summary>
    [Range(1, long.MaxValue)]
    public long TemaId { get; set; }
}
