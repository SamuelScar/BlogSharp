using BlogSharp.Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BlogSharp.Api.Data.Seeders;

public static class UsuarioSeeder
{
    private const string SenhaPadrao = "Senha@123";

    private static readonly UsuarioSeed[] Usuarios =
    [
        new(
            Nome: "Administrador BlogSharp",
            Email: "admin@blogsharp.com",
            Senha: "Admin@123",
            Tipo: "Admin",
            Foto: "https://avatars.githubusercontent.com/u/583231?s=200&v=4"),
        new(
            Nome: "Usuario BlogSharp",
            Email: "usuario@blogsharp.com",
            Senha: "Usuario@123",
            Tipo: "Usuario",
            Foto: "https://avatars.githubusercontent.com/u/9919?s=200&v=4")
    ];

    private static readonly string[] Nomes =
    [
        "Ana", "Bruno", "Carla", "Diego", "Elisa", "Felipe", "Gabriela", "Henrique",
        "Isabela", "Joao", "Laura", "Marcos", "Natalia", "Otavio", "Paula", "Rafael"
    ];

    private static readonly string[] Sobrenomes =
    [
        "Silva", "Santos", "Oliveira", "Souza", "Pereira", "Costa", "Ferreira", "Almeida",
        "Rodrigues", "Lima", "Gomes", "Ribeiro", "Carvalho", "Mendes", "Barbosa", "Rocha"
    ];

    public static async Task SeedUsuariosAsync(this IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BlogSharpDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<Usuario>>();

        await AplicarMigrationsAsync(context);

        foreach (var usuarioSeed in Usuarios)
        {
            var usuarioExiste = await context.Usuarios.AnyAsync(usuario => usuario.Email == usuarioSeed.Email);

            if (usuarioExiste)
            {
                continue;
            }

            var usuario = new Usuario
            {
                Nome = usuarioSeed.Nome,
                Email = usuarioSeed.Email,
                Tipo = usuarioSeed.Tipo,
                Foto = usuarioSeed.Foto
            };

            usuario.SenhaHash = passwordHasher.HashPassword(usuario, usuarioSeed.Senha);

            context.Usuarios.Add(usuario);
        }

        await context.SaveChangesAsync();
    }

    public static async Task<int> SeedUsuariosAleatoriosAsync(this IServiceProvider services, int quantidade)
    {
        if (quantidade <= 0)
        {
            throw new ArgumentException("Quantidade deve ser maior que zero.");
        }

        using var scope = services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BlogSharpDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<Usuario>>();

        await AplicarMigrationsAsync(context);

        for (var i = 0; i < quantidade; i++)
        {
            var nome = Nomes[Random.Shared.Next(Nomes.Length)];
            var sobrenome = Sobrenomes[Random.Shared.Next(Sobrenomes.Length)];
            var email = GerarEmail(nome, sobrenome);
            var tipo = Random.Shared.Next(10) == 0 ? "Admin" : "Usuario";
            var usuario = new Usuario
            {
                Nome = $"{nome} {sobrenome}",
                Email = email,
                Tipo = tipo,
                Foto = $"https://i.pravatar.cc/200?u={email}"
            };

            usuario.SenhaHash = passwordHasher.HashPassword(usuario, SenhaPadrao);

            context.Usuarios.Add(usuario);
        }

        return await context.SaveChangesAsync();
    }

    private static Task AplicarMigrationsAsync(BlogSharpDbContext context)
    {
        return context.Database.MigrateAsync();
    }

    private static string GerarEmail(string nome, string sobrenome)
    {
        var sufixo = Guid.NewGuid().ToString("N")[..8];

        return $"{nome}.{sobrenome}.{sufixo}@seed.blogsharp.local".ToLowerInvariant();
    }

    private sealed record UsuarioSeed(string Nome, string Email, string Senha, string Tipo, string Foto);
}
