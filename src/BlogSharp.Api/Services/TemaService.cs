using BlogSharp.Api.DTOs;
using BlogSharp.Api.Models;
using BlogSharp.Api.Repositories;

namespace BlogSharp.Api.Services;

public class TemaService(ITemaRepository temaRepository) : ITemaService
{
    public async Task<IReadOnlyList<TemaResponse>> ListarTodosAsync()
    {
        var temas = await temaRepository.ListarTodosAsync();

        return temas.Select(MapearResponse).ToList();
    }

    public async Task<TemaResponse> CadastrarAsync(TemaCadastro temaCadastro)
    {
        var tema = new Tema
        {
            Descricao = temaCadastro.Descricao
        };

        var temaCadastrado = await temaRepository.CadastrarAsync(tema);

        return MapearResponse(temaCadastrado);
    }

    public async Task<TemaResponse?> AtualizarAsync(long id, TemaAtualizacao temaAtualizacao)
    {
        var atualizado = await temaRepository.AtualizarAsync(id, temaAtualizacao.Descricao);

        if (!atualizado)
        {
            return null;
        }

        return new TemaResponse
        {
            Id = id,
            Descricao = temaAtualizacao.Descricao
        };
    }

    public async Task<bool> ExcluirAsync(long id)
    {
        return await temaRepository.ExcluirAsync(id);
    }

    private static TemaResponse MapearResponse(Tema tema)
    {
        return new TemaResponse
        {
            Id = tema.Id,
            Descricao = tema.Descricao
        };
    }
}
