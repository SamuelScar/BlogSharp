namespace BlogSharp.Api.DTOs;

public class PostagemResponse
{
    public long Id { get; set; }

    public string Titulo { get; set; } = string.Empty;

    public string Conteudo { get; set; } = string.Empty;

    public DateTime DataCriacao { get; set; }

    public DateTime? DataAtualizacao { get; set; }

    public long UsuarioId { get; set; }

    public long TemaId { get; set; }
}
