using BlogSharp.Api.Models;

namespace BlogSharp.Api.Repositories;

public interface IUsuarioRepository
{
    Task<Usuario?> BuscarPorEmailAsync(string email);

    Task<Usuario> CadastrarAsync(Usuario usuario);

    Task<bool> AtualizarAsync(long id, Usuario usuario, bool atualizarSenha);

    Task<bool> ExcluirAsync(long id);
}
