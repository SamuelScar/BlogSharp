using BlogSharp.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BlogSharp.Api.Data.Seeders;

public static class PostagemSeeder
{
    private const string EmailUsuarioPadrao = "usuario@blogsharp.com";
    private const string TemaPadrao = "Tecnologia";
    private const string TituloPadrao = "Primeira postagem BlogSharp";

    private static readonly string[] Titulos =
    [
        "Aprendendo ASP.NET Core",
        "Boas praticas em APIs",
        "Introducao ao Entity Framework",
        "Autenticacao com JWT",
        "Organizacao em camadas",
        "Persistencia com PostgreSQL"
    ];

    private static readonly string[] Conteudos =
    [
        "Postagem criada para simular conteudo de estudo no BlogSharp.",
        "Exemplo simples de publicacao para validar listagens e filtros.",
        "Conteudo gerado para testar postagens vinculadas a usuarios e temas.",
        "Registro criado pelo seeder para apoiar testes manuais da API."
    ];

    public static async Task SeedPostagensAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BlogSharpDbContext>();

        await context.Database.MigrateAsync();

        var usuarioId = await context.Usuarios
            .Where(usuario => usuario.Email == EmailUsuarioPadrao)
            .Select(usuario => usuario.Id)
            .FirstOrDefaultAsync();
        var temaId = await context.Temas
            .Where(tema => tema.Descricao == TemaPadrao)
            .Select(tema => tema.Id)
            .FirstOrDefaultAsync();

        if (usuarioId == 0 || temaId == 0)
        {
            return;
        }

        var postagemExiste = await context.Postagens.AnyAsync(postagem => postagem.Titulo == TituloPadrao);

        if (!postagemExiste)
        {
            context.Postagens.Add(new Postagem
            {
                Titulo = TituloPadrao,
                Conteudo = "Postagem inicial criada para validar o CRUD de postagens em ambiente de desenvolvimento.",
                UsuarioId = usuarioId,
                TemaId = temaId
            });

            await context.SaveChangesAsync();
        }
    }

    public static async Task<int> SeedPostagensAleatoriasAsync(this IServiceProvider services, int quantidade)
    {
        if (quantidade <= 0)
        {
            throw new ArgumentException("Quantidade deve ser maior que zero.");
        }

        await services.SeedUsuariosAsync();
        await services.SeedTemasAsync();

        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BlogSharpDbContext>();

        await context.Database.MigrateAsync();

        var usuariosIds = await context.Usuarios
            .Select(usuario => usuario.Id)
            .ToListAsync();
        var temasIds = await context.Temas
            .Select(tema => tema.Id)
            .ToListAsync();

        for (var i = 0; i < quantidade; i++)
        {
            context.Postagens.Add(new Postagem
            {
                Titulo = GerarTitulo(),
                Conteudo = GerarConteudo(),
                UsuarioId = usuariosIds[Random.Shared.Next(usuariosIds.Count)],
                TemaId = temasIds[Random.Shared.Next(temasIds.Count)]
            });
        }

        return await context.SaveChangesAsync();
    }

    private static string GerarTitulo()
    {
        var titulo = Titulos[Random.Shared.Next(Titulos.Length)];
        var sufixo = Guid.NewGuid().ToString("N")[..8];

        return $"{titulo} {sufixo}";
    }

    private static string GerarConteudo()
    {
        return Conteudos[Random.Shared.Next(Conteudos.Length)];
    }
}
