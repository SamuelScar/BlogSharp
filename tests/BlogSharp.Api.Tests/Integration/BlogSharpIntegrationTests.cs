using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using BlogSharp.Api.DTOs;
using Xunit;

namespace BlogSharp.Api.Tests.Integration;

public class BlogSharpIntegrationTests
{
    [Fact]
    public async Task CadastrarUsuario_DeveRetornarCreated()
    {
        using var factory = new BlogSharpApiFactory();
        var client = factory.CreateClient();
        var usuarioCadastro = new UsuarioCadastro
        {
            Nome = "Usuario Integracao",
            Email = "usuario.integracao@email.com",
            Senha = "Senha@123"
        };

        var response = await client.PostAsJsonAsync("/api/usuarios/cadastrar", usuarioCadastro);
        var usuario = await response.Content.ReadFromJsonAsync<UsuarioResponse>();

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        Assert.NotNull(usuario);
        Assert.Equal(usuarioCadastro.Nome, usuario!.Nome);
        Assert.Equal(usuarioCadastro.Email, usuario.Email);
        Assert.Equal("Usuario", usuario.Tipo);
    }

    [Fact]
    public async Task Login_DeveRetornarTokenValido()
    {
        using var factory = new BlogSharpApiFactory();
        var client = factory.CreateClient();
        var usuario = await factory.AdicionarUsuarioAsync(
            nome: "Usuario Login",
            email: "usuario.login@email.com",
            senha: "Senha@123");

        var response = await client.PostAsJsonAsync("/api/usuarios/login", new UsuarioLogin
        {
            Email = usuario.Email,
            Senha = "Senha@123"
        });
        var login = await response.Content.ReadFromJsonAsync<UsuarioLoginResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(login);
        Assert.Equal(usuario.Email, login!.Email);
        Assert.False(string.IsNullOrWhiteSpace(login.Token));
    }

    [Fact]
    public async Task CriarTema_DeveExigirToken()
    {
        using var factory = new BlogSharpApiFactory();
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/temas", new TemaCadastro
        {
            Descricao = "Tecnologia"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task CriarTema_DevePermitirAdminERecusarUsuarioComum()
    {
        using var factory = new BlogSharpApiFactory();
        var client = factory.CreateClient();
        var admin = await factory.AdicionarUsuarioAsync(
            nome: "Admin Integracao",
            email: "admin.integracao@email.com",
            senha: "Admin@123",
            tipo: "Admin");
        var usuario = await factory.AdicionarUsuarioAsync(
            nome: "Usuario Comum",
            email: "usuario.comum@email.com",
            senha: "Senha@123");

        Autorizar(client, await LoginAsync(client, admin.Email, "Admin@123"));
        var criado = await client.PostAsJsonAsync("/api/temas", new TemaCadastro
        {
            Descricao = "Backend"
        });

        Autorizar(client, await LoginAsync(client, usuario.Email, "Senha@123"));
        var recusado = await client.PostAsJsonAsync("/api/temas", new TemaCadastro
        {
            Descricao = "Frontend"
        });

        Assert.Equal(HttpStatusCode.Created, criado.StatusCode);
        Assert.Equal(HttpStatusCode.Forbidden, recusado.StatusCode);
    }

    [Fact]
    public async Task Postagem_DeveCadastrarEFiltrarPorAutorETema()
    {
        using var factory = new BlogSharpApiFactory();
        var client = factory.CreateClient();
        var usuario = await factory.AdicionarUsuarioAsync(
            nome: "Autor Integracao",
            email: "autor.integracao@email.com",
            senha: "Senha@123");
        var tema = await factory.AdicionarTemaAsync("Arquitetura");

        Autorizar(client, await LoginAsync(client, usuario.Email, "Senha@123"));
        var cadastro = new PostagemCadastro
        {
            Titulo = "Postagem de integracao",
            Conteudo = "Conteudo criado pelo teste de integracao.",
            UsuarioId = usuario.Id,
            TemaId = tema.Id
        };

        var criada = await client.PostAsJsonAsync("/api/postagens", cadastro);
        var filtro = await client.GetFromJsonAsync<List<PostagemResponse>>(
            $"/api/postagens/filtro?autor={usuario.Id}&tema={tema.Id}");

        Assert.Equal(HttpStatusCode.Created, criada.StatusCode);
        Assert.NotNull(filtro);
        var postagem = Assert.Single(filtro!);
        Assert.Equal(cadastro.Titulo, postagem.Titulo);
        Assert.Equal(usuario.Id, postagem.UsuarioId);
        Assert.Equal(tema.Id, postagem.TemaId);
        Assert.Equal("Resumo gerado pela IA.", postagem.ResumoIA);
        Assert.Equal("Teste, Integracao, IA", postagem.TagsIA);
        Assert.Equal("Tecnologia", postagem.CategoriaIA);
    }

    [Fact]
    public async Task IA_DeveResumirConteudoQuandoUsuarioEstaAutenticado()
    {
        using var factory = new BlogSharpApiFactory();
        var client = factory.CreateClient();
        var usuario = await factory.AdicionarUsuarioAsync(
            nome: "Usuario IA",
            email: "usuario.ia@email.com",
            senha: "Senha@123");

        Autorizar(client, await LoginAsync(client, usuario.Email, "Senha@123"));
        var response = await client.PostAsJsonAsync("/api/ia/resumir", new ResumoIARequest
        {
            Conteudo = "Conteudo para resumo inteligente de postagem."
        });
        var resultado = await response.Content.ReadFromJsonAsync<ResultadoIA>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(resultado);
        Assert.Equal("Resumo gerado pela IA.", resultado!.Resumo);
        Assert.Equal("Teste, Integracao, IA", resultado.Tags);
        Assert.Equal("Tecnologia", resultado.Categoria);
    }

    [Fact]
    public async Task Postagem_DeveRecusarCadastroParaOutroUsuario()
    {
        using var factory = new BlogSharpApiFactory();
        var client = factory.CreateClient();
        var usuarioAutenticado = await factory.AdicionarUsuarioAsync(
            nome: "Autor Autenticado",
            email: "autor.autenticado@email.com",
            senha: "Senha@123");
        var outroUsuario = await factory.AdicionarUsuarioAsync(
            nome: "Outro Autor",
            email: "outro.autor@email.com",
            senha: "Senha@123");
        var tema = await factory.AdicionarTemaAsync("Seguranca");

        Autorizar(client, await LoginAsync(client, usuarioAutenticado.Email, "Senha@123"));
        var response = await client.PostAsJsonAsync("/api/postagens", new PostagemCadastro
        {
            Titulo = "Postagem bloqueada",
            Conteudo = "Tentativa de criar postagem para outro usuario.",
            UsuarioId = outroUsuario.Id,
            TemaId = tema.Id
        });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task AtualizarPrivilegio_DevePermitirAdminAlterarUsuarioComum()
    {
        using var factory = new BlogSharpApiFactory();
        var client = factory.CreateClient();
        var admin = await factory.AdicionarUsuarioAsync(
            nome: "Admin Privilegio",
            email: "admin.privilegio@email.com",
            senha: "Admin@123",
            tipo: "Admin");
        var usuario = await factory.AdicionarUsuarioAsync(
            nome: "Usuario Promovido",
            email: "usuario.promovido@email.com",
            senha: "Senha@123");

        Autorizar(client, await LoginAsync(client, admin.Email, "Admin@123"));
        var response = await client.PatchAsJsonAsync(
            $"/api/usuarios/{usuario.Id}/privilegio",
            new UsuarioPrivilegioAtualizacao
            {
                Tipo = "Admin"
            });
        var usuarioAtualizado = await response.Content.ReadFromJsonAsync<UsuarioResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(usuarioAtualizado);
        Assert.Equal(usuario.Id, usuarioAtualizado!.Id);
        Assert.Equal("Admin", usuarioAtualizado.Tipo);
    }

    [Fact]
    public async Task AtualizarPrivilegio_DeveRecusarAdminAlterarProprioPerfil()
    {
        using var factory = new BlogSharpApiFactory();
        var client = factory.CreateClient();
        var admin = await factory.AdicionarUsuarioAsync(
            nome: "Admin Proprio Perfil",
            email: "admin.proprio@email.com",
            senha: "Admin@123",
            tipo: "Admin");

        Autorizar(client, await LoginAsync(client, admin.Email, "Admin@123"));
        var response = await client.PatchAsJsonAsync(
            $"/api/usuarios/{admin.Id}/privilegio",
            new UsuarioPrivilegioAtualizacao
            {
                Tipo = "Usuario"
            });

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    private static async Task<string> LoginAsync(HttpClient client, string email, string senha)
    {
        var response = await client.PostAsJsonAsync("/api/usuarios/login", new UsuarioLogin
        {
            Email = email,
            Senha = senha
        });
        var login = await response.Content.ReadFromJsonAsync<UsuarioLoginResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.NotNull(login);

        return login!.Token;
    }

    private static void Autorizar(HttpClient client, string token)
    {
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
    }
}
