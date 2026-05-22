namespace BlogSharp.Api.DTOs;

public class UsuarioLoginResponse
{
    public long Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Perfil de acesso do usuario autenticado.
    /// </summary>
    public string Tipo { get; set; } = string.Empty;

    public string? Foto { get; set; }

    /// <summary>
    /// Token JWT usado para acessar endpoints protegidos.
    /// </summary>
    public string Token { get; set; } = string.Empty;
}
