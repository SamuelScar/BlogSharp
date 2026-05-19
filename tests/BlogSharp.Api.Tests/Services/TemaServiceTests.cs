using BlogSharp.Api.DTOs;
using BlogSharp.Api.Models;
using BlogSharp.Api.Repositories;
using BlogSharp.Api.Services;
using Xunit;

namespace BlogSharp.Api.Tests.Services;

public class TemaServiceTests
{
    [Fact]
    public async Task ListarTodosAsync_DeveRetornarTemasCadastrados()
    {
        var repository = new FakeTemaRepository();
        repository.AdicionarTema(new Tema { Descricao = "Tecnologia" });
        repository.AdicionarTema(new Tema { Descricao = "Programacao" });
        var service = new TemaService(repository);

        var temas = await service.ListarTodosAsync();

        Assert.Equal(2, temas.Count);
        Assert.Equal("Tecnologia", temas[0].Descricao);
        Assert.Equal("Programacao", temas[1].Descricao);
    }

    [Fact]
    public async Task CadastrarAsync_DeveCadastrarTema()
    {
        var repository = new FakeTemaRepository();
        var service = new TemaService(repository);
        var temaCadastro = new TemaCadastro
        {
            Descricao = "Dotnet"
        };

        var response = await service.CadastrarAsync(temaCadastro);

        Assert.Equal(1, response.Id);
        Assert.Equal(temaCadastro.Descricao, response.Descricao);
    }

    [Fact]
    public async Task AtualizarAsync_DeveAtualizarTemaQuandoExiste()
    {
        var repository = new FakeTemaRepository();
        var tema = repository.AdicionarTema(new Tema { Descricao = "Antigo" });
        var service = new TemaService(repository);
        var temaAtualizacao = new TemaAtualizacao
        {
            Descricao = "Novo"
        };

        var response = await service.AtualizarAsync(tema.Id, temaAtualizacao);

        Assert.NotNull(response);
        Assert.Equal(tema.Id, response!.Id);
        Assert.Equal(temaAtualizacao.Descricao, response.Descricao);
        Assert.Equal(temaAtualizacao.Descricao, tema.Descricao);
    }

    [Fact]
    public async Task AtualizarAsync_DeveRetornarNullQuandoTemaNaoExiste()
    {
        var service = new TemaService(new FakeTemaRepository());
        var temaAtualizacao = new TemaAtualizacao
        {
            Descricao = "Novo"
        };

        var response = await service.AtualizarAsync(99, temaAtualizacao);

        Assert.Null(response);
    }

    [Fact]
    public async Task ExcluirAsync_DeveExcluirTemaQuandoExiste()
    {
        var repository = new FakeTemaRepository();
        var tema = repository.AdicionarTema(new Tema { Descricao = "Tecnologia" });
        var service = new TemaService(repository);

        var excluido = await service.ExcluirAsync(tema.Id);
        var temas = await repository.ListarTodosAsync();

        Assert.True(excluido);
        Assert.DoesNotContain(temas, temaSalvo => temaSalvo.Id == tema.Id);
    }

    [Fact]
    public async Task ExcluirAsync_DeveRetornarFalseQuandoTemaNaoExiste()
    {
        var service = new TemaService(new FakeTemaRepository());

        var excluido = await service.ExcluirAsync(99);

        Assert.False(excluido);
    }

    private sealed class FakeTemaRepository : ITemaRepository
    {
        private readonly List<Tema> temas = [];
        private long proximoId = 1;

        public Task<IReadOnlyList<Tema>> ListarTodosAsync()
        {
            return Task.FromResult<IReadOnlyList<Tema>>(temas.OrderBy(tema => tema.Id).ToList());
        }

        public Task<Tema> CadastrarAsync(Tema tema)
        {
            AdicionarTema(tema);

            return Task.FromResult(tema);
        }

        public Task<bool> AtualizarAsync(long id, string descricao)
        {
            var tema = temas.FirstOrDefault(tema => tema.Id == id);

            if (tema is null)
            {
                return Task.FromResult(false);
            }

            tema.Descricao = descricao;

            return Task.FromResult(true);
        }

        public Task<bool> ExcluirAsync(long id)
        {
            var tema = temas.FirstOrDefault(tema => tema.Id == id);

            if (tema is null)
            {
                return Task.FromResult(false);
            }

            temas.Remove(tema);

            return Task.FromResult(true);
        }

        public Tema AdicionarTema(Tema tema)
        {
            tema.Id = tema.Id == 0 ? proximoId++ : tema.Id;
            temas.Add(tema);

            return tema;
        }
    }
}
