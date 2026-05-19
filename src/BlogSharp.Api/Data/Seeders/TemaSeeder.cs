using BlogSharp.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BlogSharp.Api.Data.Seeders;

public static class TemaSeeder
{
    private const string TemaPadrao = "Tecnologia";

    private static readonly string[] TemasBase =
    [
        "Tecnologia",
        "Programacao",
        "Backend",
        "Dotnet"
    ];

    public static async Task SeedTemasAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BlogSharpDbContext>();

        await context.Database.MigrateAsync();

        var temaExiste = await context.Temas.AnyAsync(tema => tema.Descricao == TemaPadrao);

        if (!temaExiste)
        {
            context.Temas.Add(new Tema { Descricao = TemaPadrao });
            await context.SaveChangesAsync();
        }
    }

    public static async Task<int> SeedTemasAleatoriosAsync(this IServiceProvider services, int quantidade)
    {
        if (quantidade <= 0)
        {
            throw new ArgumentException("Quantidade deve ser maior que zero.");
        }

        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BlogSharpDbContext>();

        await context.Database.MigrateAsync();

        for (var i = 0; i < quantidade; i++)
        {
            context.Temas.Add(new Tema { Descricao = GerarDescricao() });
        }

        return await context.SaveChangesAsync();
    }

    private static string GerarDescricao()
    {
        var temaBase = TemasBase[Random.Shared.Next(TemasBase.Length)];
        var sufixo = Guid.NewGuid().ToString("N")[..8];

        return $"{temaBase} {sufixo}";
    }
}
