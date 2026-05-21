namespace BlogSharp.Api.DTOs;

public class PostagemFiltro
{
    /// <summary>
    /// Identificador do autor usado para filtrar postagens.
    /// </summary>
    public long? Autor { get; set; }

    /// <summary>
    /// Identificador do tema usado para filtrar postagens.
    /// </summary>
    public long? Tema { get; set; }
}
