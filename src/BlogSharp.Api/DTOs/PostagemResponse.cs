namespace BlogSharp.Api.DTOs;

public class PostagemResponse
{
    public long Id { get; set; }

    public string Titulo { get; set; } = string.Empty;

    public string Conteudo { get; set; } = string.Empty;

    public DateTime DataCriacao { get; set; }

    public DateTime? DataAtualizacao { get; set; }

    /// <summary>
    /// Resumo gerado pela IA quando a integracao estiver habilitada.
    /// </summary>
    public string? ResumoIA { get; set; }

    /// <summary>
    /// Palavras-chave geradas pela IA quando a integracao estiver habilitada.
    /// </summary>
    public string? TagsIA { get; set; }

    /// <summary>
    /// Categoria sugerida pela IA quando a integracao estiver habilitada.
    /// </summary>
    public string? CategoriaIA { get; set; }

    public long UsuarioId { get; set; }

    public long TemaId { get; set; }
}
