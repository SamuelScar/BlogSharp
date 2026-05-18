using BlogSharp.Api.Models;

namespace BlogSharp.Api.Repositories;

public interface IUsuarioRepository
{
    Task<Usuario?> BuscarPorIdAsync(long id);

    Task<Usuario?> BuscarPorEmailAsync(string email);

    Task<Usuario> CadastrarAsync(Usuario usuario);

    Task<Usuario> AtualizarAsync(Usuario usuario);

    Task ExcluirAsync(Usuario usuario);
}
