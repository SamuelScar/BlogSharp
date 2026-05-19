using System.ComponentModel.DataAnnotations;

namespace BlogSharp.Api.DTOs;

public class PostagemCadastro
{
    [Required]
    [StringLength(100)]
    public string Titulo { get; set; } = string.Empty;

    [Required]
    [StringLength(5000)]
    public string Conteudo { get; set; } = string.Empty;

    [Range(1, long.MaxValue)]
    public long UsuarioId { get; set; }

    [Range(1, long.MaxValue)]
    public long TemaId { get; set; }
}
