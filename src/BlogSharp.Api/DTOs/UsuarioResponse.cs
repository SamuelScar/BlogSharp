namespace BlogSharp.Api.DTOs;

public class UsuarioResponse
{
    public long Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Tipo { get; set; } = string.Empty;

    public string? Foto { get; set; }
}
