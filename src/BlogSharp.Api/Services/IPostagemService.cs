using BlogSharp.Api.DTOs;

namespace BlogSharp.Api.Services;

public interface IPostagemService
{
    Task<IReadOnlyList<PostagemResponse>> ListarTodasAsync();

    Task<IReadOnlyList<PostagemResponse>> FiltrarAsync(PostagemFiltro filtro);

    Task<PostagemResponse> CadastrarAsync(PostagemCadastro postagemCadastro);

    Task<PostagemResponse?> AtualizarAsync(long id, PostagemAtualizacao postagemAtualizacao);

    Task<bool> ExcluirAsync(long id);

    Task<long?> BuscarUsuarioIdAsync(long id);
}
