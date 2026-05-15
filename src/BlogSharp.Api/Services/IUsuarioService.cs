using BlogSharp.Api.DTOs;
using BlogSharp.Api.Models;

namespace BlogSharp.Api.Services;

public interface IUsuarioService
{
    Task<Usuario> CadastrarAsync(Usuario usuario);

    Task<Usuario?> AtualizarAsync(long id, Usuario usuario);

    Task<bool> ExcluirAsync(long id);

    Task<UsuarioLogin?> AutenticarAsync(UsuarioLogin usuarioLogin);
}
