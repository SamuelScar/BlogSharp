using BlogSharp.Api.Models;

namespace BlogSharp.Api.Services;

public interface ITemaService
{
    Task<IReadOnlyList<Tema>> ListarTodosAsync();

    Task<Tema> CadastrarAsync(Tema tema);

    Task<Tema?> AtualizarAsync(long id, Tema tema);

    Task<bool> ExcluirAsync(long id);
}
