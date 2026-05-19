using BlogSharp.Api.Models;

namespace BlogSharp.Api.Repositories;

public interface ITemaRepository
{
    Task<IReadOnlyList<Tema>> ListarTodosAsync();

    Task<Tema> CadastrarAsync(Tema tema);

    Task<bool> AtualizarAsync(long id, string descricao);

    Task<bool> ExcluirAsync(long id);
}
