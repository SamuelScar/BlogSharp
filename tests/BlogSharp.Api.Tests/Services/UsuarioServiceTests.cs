using BlogSharp.Api.DTOs;
using BlogSharp.Api.Models;
using BlogSharp.Api.Repositories;
using BlogSharp.Api.Services;
using Microsoft.AspNetCore.Identity;
using Xunit;

namespace BlogSharp.Api.Tests.Services;

public class UsuarioServiceTests
{
    private const string FotoUrl = "https://avatars.githubusercontent.com/u/9919?s=200&v=4";

    [Fact]
    public async Task CadastrarAsync_DeveCadastrarUsuarioComSenhaHasheada()
    {
        var repository = new FakeUsuarioRepository();
        var service = CriarService(repository);
        var usuarioCadastro = new UsuarioCadastro
        {
            Nome = "Samuel Teste",
            Email = "samuel.teste@email.com",
            Senha = "123456",
            Tipo = "Usuario",
            Foto = FotoUrl
        };

        var response = await service.CadastrarAsync(usuarioCadastro);

        var usuarioSalvo = await repository.BuscarPorEmailAsync(usuarioCadastro.Email);
        Assert.NotNull(usuarioSalvo);
        Assert.Equal(1, response.Id);
        Assert.Equal(usuarioCadastro.Nome, response.Nome);
        Assert.Equal(usuarioCadastro.Email, response.Email);
        Assert.Equal(usuarioCadastro.Tipo, response.Tipo);
        Assert.Equal(usuarioCadastro.Foto, response.Foto);
        Assert.NotEqual(usuarioCadastro.Senha, usuarioSalvo!.SenhaHash);
        Assert.Equal(
            PasswordVerificationResult.Success,
            new PasswordHasher<Usuario>().VerifyHashedPassword(usuarioSalvo, usuarioSalvo.SenhaHash, usuarioCadastro.Senha));
    }

    [Fact]
    public async Task CadastrarAsync_DeveRecusarEmailJaCadastrado()
    {
        var repository = new FakeUsuarioRepository();
        repository.AdicionarUsuario(new Usuario
        {
            Nome = "Usuario Existente",
            Email = "existente@email.com",
            SenhaHash = "hash",
            Tipo = "Usuario"
        });
        var service = CriarService(repository);
        var usuarioCadastro = new UsuarioCadastro
        {
            Nome = "Novo Usuario",
            Email = "existente@email.com",
            Senha = "123456",
            Tipo = "Usuario"
        };

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => service.CadastrarAsync(usuarioCadastro));

        Assert.Equal("Email ja cadastrado.", exception.Message);
    }

    [Fact]
    public async Task AtualizarAsync_DeveAtualizarDadosESenhaQuandoUsuarioExiste()
    {
        var repository = new FakeUsuarioRepository();
        var passwordHasher = new PasswordHasher<Usuario>();
        var usuario = new Usuario
        {
            Nome = "Nome Antigo",
            Email = "antigo@email.com",
            Tipo = "Usuario",
            Foto = "https://example.com/antiga.png"
        };
        usuario.SenhaHash = passwordHasher.HashPassword(usuario, "123456");
        repository.AdicionarUsuario(usuario);
        var service = CriarService(repository);
        var usuarioAtualizacao = new UsuarioAtualizacao
        {
            Nome = "Nome Novo",
            Email = "novo@email.com",
            Senha = "654321",
            Tipo = "Admin",
            Foto = "https://example.com/nova.png"
        };

        var response = await service.AtualizarAsync(usuario.Id, usuarioAtualizacao);

        Assert.NotNull(response);
        Assert.Equal(usuarioAtualizacao.Nome, response!.Nome);
        Assert.Equal(usuarioAtualizacao.Email, response.Email);
        Assert.Equal(usuarioAtualizacao.Tipo, response.Tipo);
        Assert.Equal(usuarioAtualizacao.Foto, response.Foto);
        Assert.Equal(
            PasswordVerificationResult.Success,
            passwordHasher.VerifyHashedPassword(usuario, usuario.SenhaHash, usuarioAtualizacao.Senha));
    }

    [Fact]
    public async Task AtualizarAsync_DeveRetornarNullQuandoUsuarioNaoExiste()
    {
        var service = CriarService(new FakeUsuarioRepository());
        var usuarioAtualizacao = new UsuarioAtualizacao
        {
            Nome = "Nome",
            Email = "email@email.com",
            Tipo = "Usuario"
        };

        var response = await service.AtualizarAsync(99, usuarioAtualizacao);

        Assert.Null(response);
    }

    [Fact]
    public async Task ExcluirAsync_DeveExcluirQuandoUsuarioExiste()
    {
        var repository = new FakeUsuarioRepository();
        var usuario = repository.AdicionarUsuario(new Usuario
        {
            Nome = "Usuario",
            Email = "usuario@email.com",
            SenhaHash = "hash",
            Tipo = "Usuario"
        });
        var service = CriarService(repository);

        var excluido = await service.ExcluirAsync(usuario.Id);

        Assert.True(excluido);
        Assert.Null(await repository.BuscarPorIdAsync(usuario.Id));
    }

    [Fact]
    public async Task AutenticarAsync_DeveRetornarTokenQuandoCredenciaisSaoValidas()
    {
        var repository = new FakeUsuarioRepository();
        var passwordHasher = new PasswordHasher<Usuario>();
        var usuario = new Usuario
        {
            Nome = "Samuel Teste",
            Email = "samuel.teste@email.com",
            Tipo = "Usuario",
            Foto = FotoUrl
        };
        usuario.SenhaHash = passwordHasher.HashPassword(usuario, "123456");
        repository.AdicionarUsuario(usuario);
        var service = CriarService(repository);
        var usuarioLogin = new UsuarioLogin
        {
            Email = usuario.Email,
            Senha = "123456"
        };

        var response = await service.AutenticarAsync(usuarioLogin);

        Assert.NotNull(response);
        Assert.Equal(usuario.Id, response!.Id);
        Assert.Equal(usuario.Email, response.Email);
        Assert.Equal($"token-{usuario.Id}", response.Token);
    }

    [Fact]
    public async Task AutenticarAsync_DeveRetornarNullQuandoSenhaEInvalida()
    {
        var repository = new FakeUsuarioRepository();
        var passwordHasher = new PasswordHasher<Usuario>();
        var usuario = new Usuario
        {
            Nome = "Samuel Teste",
            Email = "samuel.teste@email.com",
            Tipo = "Usuario"
        };
        usuario.SenhaHash = passwordHasher.HashPassword(usuario, "123456");
        repository.AdicionarUsuario(usuario);
        var service = CriarService(repository);
        var usuarioLogin = new UsuarioLogin
        {
            Email = usuario.Email,
            Senha = "senha-errada"
        };

        var response = await service.AutenticarAsync(usuarioLogin);

        Assert.Null(response);
    }

    private static UsuarioService CriarService(FakeUsuarioRepository repository)
    {
        return new UsuarioService(
            repository,
            new PasswordHasher<Usuario>(),
            new FakeTokenService());
    }

    private sealed class FakeUsuarioRepository : IUsuarioRepository
    {
        private readonly List<Usuario> usuarios = [];
        private long proximoId = 1;

        public Task<Usuario?> BuscarPorIdAsync(long id)
        {
            return Task.FromResult(usuarios.FirstOrDefault(usuario => usuario.Id == id));
        }

        public Task<Usuario?> BuscarPorEmailAsync(string email)
        {
            return Task.FromResult(usuarios.FirstOrDefault(usuario => usuario.Email == email));
        }

        public Task<Usuario> CadastrarAsync(Usuario usuario)
        {
            AdicionarUsuario(usuario);

            return Task.FromResult(usuario);
        }

        public Task<Usuario> AtualizarAsync(Usuario usuario)
        {
            return Task.FromResult(usuario);
        }

        public Task ExcluirAsync(Usuario usuario)
        {
            usuarios.Remove(usuario);

            return Task.CompletedTask;
        }

        public Usuario AdicionarUsuario(Usuario usuario)
        {
            usuario.Id = usuario.Id == 0 ? proximoId++ : usuario.Id;
            usuarios.Add(usuario);

            return usuario;
        }
    }

    private sealed class FakeTokenService : ITokenService
    {
        public string GerarToken(Usuario usuario)
        {
            return $"token-{usuario.Id}";
        }
    }
}
