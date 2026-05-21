using System.ComponentModel.DataAnnotations;

namespace BlogSharp.Api.Models;

public class Postagem
{
    public long Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Titulo { get; set; } = string.Empty;

    [Required]
    [StringLength(5000)]
    public string Conteudo { get; set; } = string.Empty;

    public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

    public DateTime? DataAtualizacao { get; set; }

    [StringLength(500)]
    public string? ResumoIA { get; set; }

    [StringLength(500)]
    public string? TagsIA { get; set; }

    [StringLength(100)]
    public string? CategoriaIA { get; set; }

    public long UsuarioId { get; set; }

    public Usuario Usuario { get; set; } = null!;

    public long TemaId { get; set; }

    public Tema Tema { get; set; } = null!;
}
