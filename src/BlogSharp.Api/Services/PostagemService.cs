using BlogSharp.Api.DTOs;
using BlogSharp.Api.Exceptions;
using BlogSharp.Api.Models;
using BlogSharp.Api.Repositories;
using BlogSharp.Api.Services.IA;
using Microsoft.Extensions.Options;

namespace BlogSharp.Api.Services;

public class PostagemService(
    IPostagemRepository postagemRepository,
    IIAService iaService,
    IOptions<IAOptions> iaOptions) : IPostagemService
{
    public async Task<IReadOnlyList<PostagemResponse>> ListarTodasAsync()
    {
        var postagens = await postagemRepository.ListarTodasAsync();

        return postagens.Select(MapearResponse).ToList();
    }

    public async Task<IReadOnlyList<PostagemResponse>> FiltrarAsync(PostagemFiltro filtro)
    {
        var postagens = await postagemRepository.FiltrarAsync(filtro.Autor, filtro.Tema);

        return postagens.Select(MapearResponse).ToList();
    }

    public async Task<PostagemResponse> CadastrarAsync(PostagemCadastro postagemCadastro)
    {
        await ValidarRelacionamentosAsync(postagemCadastro.UsuarioId, postagemCadastro.TemaId);
        var resultadoIA = await GerarResumoIAAsync(postagemCadastro.Conteudo);

        var postagem = new Postagem
        {
            Titulo = postagemCadastro.Titulo,
            Conteudo = postagemCadastro.Conteudo,
            ResumoIA = resultadoIA?.Resumo,
            TagsIA = resultadoIA?.Tags,
            CategoriaIA = resultadoIA?.Categoria,
            UsuarioId = postagemCadastro.UsuarioId,
            TemaId = postagemCadastro.TemaId
        };

        var postagemCadastrada = await postagemRepository.CadastrarAsync(postagem);

        return MapearResponse(postagemCadastrada);
    }

    private async Task<ResultadoIA?> GerarResumoIAAsync(string conteudo)
    {
        return iaOptions.Value.Enabled
            ? await iaService.GerarResumoAsync(conteudo)
            : null;
    }

    public async Task<PostagemResponse?> AtualizarAsync(long id, PostagemAtualizacao postagemAtualizacao)
    {
        await ValidarRelacionamentosAsync(postagemAtualizacao.UsuarioId, postagemAtualizacao.TemaId);

        var postagem = new Postagem
        {
            Titulo = postagemAtualizacao.Titulo,
            Conteudo = postagemAtualizacao.Conteudo,
            UsuarioId = postagemAtualizacao.UsuarioId,
            TemaId = postagemAtualizacao.TemaId
        };

        var postagemAtualizada = await postagemRepository.AtualizarAsync(id, postagem);

        return postagemAtualizada is null ? null : MapearResponse(postagemAtualizada);
    }

    public async Task<bool> ExcluirAsync(long id)
    {
        return await postagemRepository.ExcluirAsync(id);
    }

    public async Task<long?> BuscarUsuarioIdAsync(long id)
    {
        return await postagemRepository.BuscarUsuarioIdAsync(id);
    }

    private async Task ValidarRelacionamentosAsync(long usuarioId, long temaId)
    {
        if (!await postagemRepository.UsuarioExisteAsync(usuarioId))
        {
            throw new RecursoNaoEncontradoException("Usuario nao encontrado.");
        }

        if (!await postagemRepository.TemaExisteAsync(temaId))
        {
            throw new RecursoNaoEncontradoException("Tema nao encontrado.");
        }
    }

    private static PostagemResponse MapearResponse(Postagem postagem)
    {
        return new PostagemResponse
        {
            Id = postagem.Id,
            Titulo = postagem.Titulo,
            Conteudo = postagem.Conteudo,
            DataCriacao = postagem.DataCriacao,
            DataAtualizacao = postagem.DataAtualizacao,
            ResumoIA = postagem.ResumoIA,
            TagsIA = postagem.TagsIA,
            CategoriaIA = postagem.CategoriaIA,
            UsuarioId = postagem.UsuarioId,
            TemaId = postagem.TemaId
        };
    }
}
