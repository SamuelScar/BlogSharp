using BlogSharp.Api.Models;

namespace BlogSharp.Api.Repositories;

public interface IUsuarioRepository
{
    Task<Usuario?> BuscarPorEmailAsync(string email);

    Task<Usuario> CadastrarAsync(Usuario usuario);

    Task<Usuario?> AtualizarAsync(long id, Usuario usuarioAtualizado, bool atualizarSenha);

    Task<Usuario?> AtualizarTipoAsync(long id, string tipo);

    Task<bool> ExcluirAsync(long id);
}
