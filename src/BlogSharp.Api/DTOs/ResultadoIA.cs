namespace BlogSharp.Api.DTOs;

public class ResultadoIA
{
    /// <summary>
    /// Resumo curto gerado a partir do conteudo da postagem.
    /// </summary>
    public string Resumo { get; set; } = string.Empty;

    /// <summary>
    /// Palavras-chave relacionadas ao conteudo, separadas por virgula.
    /// </summary>
    public string Tags { get; set; } = string.Empty;

    /// <summary>
    /// Categoria sugerida para classificar a postagem.
    /// </summary>
    public string Categoria { get; set; } = string.Empty;
}
