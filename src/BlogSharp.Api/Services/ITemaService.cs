using BlogSharp.Api.DTOs;

namespace BlogSharp.Api.Services;

public interface ITemaService
{
    Task<IReadOnlyList<TemaResponse>> ListarTodosAsync();

    Task<TemaResponse> CadastrarAsync(TemaCadastro temaCadastro);

    Task<TemaResponse?> AtualizarAsync(long id, TemaAtualizacao temaAtualizacao);

    Task<bool> ExcluirAsync(long id);
}
