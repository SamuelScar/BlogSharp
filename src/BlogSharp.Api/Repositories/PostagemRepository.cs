using BlogSharp.Api.Data;
using BlogSharp.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BlogSharp.Api.Repositories;

public class PostagemRepository(BlogSharpDbContext context) : IPostagemRepository
{
    public async Task<IReadOnlyList<Postagem>> ListarTodasAsync()
    {
        return await CriarConsulta()
            .OrderBy(postagem => postagem.Id)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<Postagem>> FiltrarAsync(long? autorId, long? temaId)
    {
        var query = CriarConsulta();

        if (autorId.HasValue)
        {
            query = query.Where(postagem => postagem.UsuarioId == autorId.Value);
        }

        if (temaId.HasValue)
        {
            query = query.Where(postagem => postagem.TemaId == temaId.Value);
        }

        return await query
            .OrderBy(postagem => postagem.Id)
            .ToListAsync();
    }

    public async Task<Postagem> CadastrarAsync(Postagem postagem)
    {
        context.Postagens.Add(postagem);
        await context.SaveChangesAsync();

        return postagem;
    }

    public async Task<Postagem?> AtualizarAsync(long id, Postagem dadosPostagem)
    {
        var postagemSalva = await context.Postagens.FirstOrDefaultAsync(postagem => postagem.Id == id);

        if (postagemSalva is null)
        {
            return null;
        }

        postagemSalva.Titulo = dadosPostagem.Titulo;
        postagemSalva.Conteudo = dadosPostagem.Conteudo;
        postagemSalva.UsuarioId = dadosPostagem.UsuarioId;
        postagemSalva.TemaId = dadosPostagem.TemaId;
        postagemSalva.DataAtualizacao = DateTime.UtcNow;

        await context.SaveChangesAsync();

        return postagemSalva;
    }

    public async Task<bool> ExcluirAsync(long id)
    {
        var registrosExcluidos = await context.Postagens
            .Where(postagem => postagem.Id == id)
            .ExecuteDeleteAsync();

        return registrosExcluidos > 0;
    }

    public Task<bool> UsuarioExisteAsync(long usuarioId)
    {
        return context.Usuarios.AnyAsync(usuario => usuario.Id == usuarioId);
    }

    public Task<bool> TemaExisteAsync(long temaId)
    {
        return context.Temas.AnyAsync(tema => tema.Id == temaId);
    }

    private IQueryable<Postagem> CriarConsulta()
    {
        return context.Postagens
            .AsNoTracking();
    }
}
