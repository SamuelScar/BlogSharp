using BlogSharp.Api.Models;

namespace BlogSharp.Api.Repositories;

public interface IPostagemRepository
{
    Task<IReadOnlyList<Postagem>> ListarTodasAsync();

    Task<IReadOnlyList<Postagem>> FiltrarAsync(long? autorId, long? temaId);

    Task<Postagem> CadastrarAsync(Postagem postagem);

    Task<Postagem?> AtualizarAsync(long id, Postagem postagem);

    Task<bool> ExcluirAsync(long id);
}
