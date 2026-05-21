using BlogSharp.Api.DTOs;

namespace BlogSharp.Api.Services;

public interface IUsuarioService
{
    Task<UsuarioResponse> CadastrarAsync(UsuarioCadastro usuarioCadastro);

    Task<UsuarioResponse?> AtualizarAsync(long id, UsuarioAtualizacao usuarioAtualizacao);

    Task<UsuarioResponse?> AtualizarPrivilegioAsync(long id, UsuarioPrivilegioAtualizacao usuarioPrivilegioAtualizacao);

    Task<bool> ExcluirAsync(long id);

    Task<UsuarioLoginResponse?> AutenticarAsync(UsuarioLogin usuarioLogin);
}
