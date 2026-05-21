using BlogSharp.Api.Data;
using BlogSharp.Api.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace BlogSharp.Api.Tests.Integration;

public class BlogSharpApiFactory : WebApplicationFactory<Program>
{
    private const string JwtSecretKey = "chave_de_teste_com_mais_de_32_caracteres";
    private const string JwtIssuer = "BlogSharp.Api";
    private const string JwtAudience = "BlogSharp.Api";

    private readonly string databaseName = $"blogsharp-tests-{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:SecretKey"] = JwtSecretKey,
                ["Jwt:Issuer"] = JwtIssuer,
                ["Jwt:Audience"] = JwtAudience,
                ["Jwt:ExpirationMinutes"] = "60"
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<BlogSharpDbContext>>();
            services.AddDbContext<BlogSharpDbContext>(options => options.UseInMemoryDatabase(databaseName));

            services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(JwtSecretKey)),
                        ValidateIssuer = true,
                        ValidIssuer = JwtIssuer,
                        ValidateAudience = true,
                        ValidAudience = JwtAudience,
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero
                    };
                });
        });
    }

    public async Task<Usuario> AdicionarUsuarioAsync(
        string nome,
        string email,
        string senha,
        string tipo = "Usuario")
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BlogSharpDbContext>();
        var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<Usuario>>();
        var usuario = new Usuario
        {
            Nome = nome,
            Email = email,
            Tipo = tipo
        };
        usuario.SenhaHash = passwordHasher.HashPassword(usuario, senha);

        context.Usuarios.Add(usuario);
        await context.SaveChangesAsync();

        return usuario;
    }

    public async Task<Tema> AdicionarTemaAsync(string descricao)
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BlogSharpDbContext>();
        var tema = new Tema { Descricao = descricao };

        context.Temas.Add(tema);
        await context.SaveChangesAsync();

        return tema;
    }
}
