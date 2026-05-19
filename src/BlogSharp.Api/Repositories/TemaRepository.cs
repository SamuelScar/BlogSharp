using BlogSharp.Api.Data;
using BlogSharp.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BlogSharp.Api.Repositories;

public class TemaRepository(BlogSharpDbContext context) : ITemaRepository
{
    public async Task<IReadOnlyList<Tema>> ListarTodosAsync()
    {
        return await context.Temas
            .OrderBy(tema => tema.Id)
            .ToListAsync();
    }

    public async Task<Tema> CadastrarAsync(Tema tema)
    {
        context.Temas.Add(tema);
        await context.SaveChangesAsync();

        return tema;
    }

    public async Task<bool> AtualizarAsync(long id, string descricao)
    {
        var registrosAtualizados = await context.Temas
            .Where(tema => tema.Id == id)
            .ExecuteUpdateAsync(setter => setter
                .SetProperty(tema => tema.Descricao, descricao));

        return registrosAtualizados > 0;
    }

    public async Task<bool> ExcluirAsync(long id)
    {
        var registrosExcluidos = await context.Temas
            .Where(tema => tema.Id == id)
            .ExecuteDeleteAsync();

        return registrosExcluidos > 0;
    }
}
