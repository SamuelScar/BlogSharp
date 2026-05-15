using BlogSharp.Api.Models;

namespace BlogSharp.Api.Repositories;

public interface IUsuarioRepository
{
    Task<Usuario?> BuscarPorEmailAsync(string email);

    Task<Usuario> CadastrarAsync(Usuario usuario);

    Task<Usuario?> AtualizarAsync(long id, Usuario usuario);

    Task<bool> ExcluirAsync(long id);
}
