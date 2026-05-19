using BlogSharp.Api.DTOs;
using BlogSharp.Api.Models;
using BlogSharp.Api.Repositories;
using BlogSharp.Api.Services;
using Xunit;

namespace BlogSharp.Api.Tests.Services;

public class PostagemServiceTests
{
    [Fact]
    public async Task ListarTodasAsync_DeveRetornarPostagensCadastradas()
    {
        var repository = new FakePostagemRepository();
        repository.AdicionarPostagem(new Postagem
        {
            Titulo = "Primeira postagem",
            Conteudo = "Conteudo da primeira postagem",
            UsuarioId = 1,
            TemaId = 1
        });
        repository.AdicionarPostagem(new Postagem
        {
            Titulo = "Segunda postagem",
            Conteudo = "Conteudo da segunda postagem",
            UsuarioId = 2,
            TemaId = 1
        });
        var service = new PostagemService(repository);

        var postagens = await service.ListarTodasAsync();

        Assert.Equal(2, postagens.Count);
        Assert.Equal("Primeira postagem", postagens[0].Titulo);
        Assert.Equal("Segunda postagem", postagens[1].Titulo);
    }

    [Fact]
    public async Task FiltrarAsync_DeveFiltrarPorAutor()
    {
        var repository = CriarRepositoryComPostagens();
        var service = new PostagemService(repository);

        var postagens = await service.FiltrarAsync(new PostagemFiltro { Autor = 1 });

        Assert.Equal(2, postagens.Count);
        Assert.All(postagens, postagem => Assert.Equal(1, postagem.UsuarioId));
    }

    [Fact]
    public async Task FiltrarAsync_DeveFiltrarPorTema()
    {
        var repository = CriarRepositoryComPostagens();
        var service = new PostagemService(repository);

        var postagens = await service.FiltrarAsync(new PostagemFiltro { Tema = 1 });

        Assert.Equal(2, postagens.Count);
        Assert.All(postagens, postagem => Assert.Equal(1, postagem.TemaId));
    }

    [Fact]
    public async Task FiltrarAsync_DeveFiltrarPorAutorETema()
    {
        var repository = CriarRepositoryComPostagens();
        var service = new PostagemService(repository);

        var postagens = await service.FiltrarAsync(new PostagemFiltro { Autor = 1, Tema = 2 });

        Assert.Single(postagens);
        Assert.Equal(1, postagens[0].UsuarioId);
        Assert.Equal(2, postagens[0].TemaId);
    }

    [Fact]
    public async Task CadastrarAsync_DeveCadastrarPostagemQuandoUsuarioETemaExistem()
    {
        var repository = new FakePostagemRepository();
        repository.AdicionarUsuario(1);
        repository.AdicionarTema(1);
        var service = new PostagemService(repository);
        var postagemCadastro = new PostagemCadastro
        {
            Titulo = "Postagem de teste",
            Conteudo = "Conteudo da postagem de teste",
            UsuarioId = 1,
            TemaId = 1
        };

        var response = await service.CadastrarAsync(postagemCadastro);

        Assert.Equal(1, response.Id);
        Assert.Equal(postagemCadastro.Titulo, response.Titulo);
        Assert.Equal(postagemCadastro.Conteudo, response.Conteudo);
        Assert.Equal(postagemCadastro.UsuarioId, response.UsuarioId);
        Assert.Equal(postagemCadastro.TemaId, response.TemaId);
        Assert.NotEqual(default, response.DataCriacao);
    }

    [Fact]
    public async Task CadastrarAsync_DeveRecusarUsuarioInexistente()
    {
        var repository = new FakePostagemRepository();
        repository.AdicionarTema(1);
        var service = new PostagemService(repository);
        var postagemCadastro = new PostagemCadastro
        {
            Titulo = "Postagem de teste",
            Conteudo = "Conteudo da postagem de teste",
            UsuarioId = 99,
            TemaId = 1
        };

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CadastrarAsync(postagemCadastro));

        Assert.Equal("Usuario nao encontrado.", exception.Message);
        Assert.Empty(await repository.ListarTodasAsync());
    }

    [Fact]
    public async Task CadastrarAsync_DeveRecusarTemaInexistente()
    {
        var repository = new FakePostagemRepository();
        repository.AdicionarUsuario(1);
        var service = new PostagemService(repository);
        var postagemCadastro = new PostagemCadastro
        {
            Titulo = "Postagem de teste",
            Conteudo = "Conteudo da postagem de teste",
            UsuarioId = 1,
            TemaId = 99
        };

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.CadastrarAsync(postagemCadastro));

        Assert.Equal("Tema nao encontrado.", exception.Message);
        Assert.Empty(await repository.ListarTodasAsync());
    }

    [Fact]
    public async Task AtualizarAsync_DeveAtualizarPostagemQuandoExiste()
    {
        var repository = new FakePostagemRepository();
        repository.AdicionarUsuario(1);
        repository.AdicionarUsuario(2);
        repository.AdicionarTema(1);
        repository.AdicionarTema(2);
        var postagem = repository.AdicionarPostagem(new Postagem
        {
            Titulo = "Titulo antigo",
            Conteudo = "Conteudo antigo",
            UsuarioId = 1,
            TemaId = 1
        });
        var service = new PostagemService(repository);
        var postagemAtualizacao = new PostagemAtualizacao
        {
            Titulo = "Titulo novo",
            Conteudo = "Conteudo novo",
            UsuarioId = 2,
            TemaId = 2
        };

        var response = await service.AtualizarAsync(postagem.Id, postagemAtualizacao);

        Assert.NotNull(response);
        Assert.Equal(postagem.Id, response!.Id);
        Assert.Equal(postagemAtualizacao.Titulo, response.Titulo);
        Assert.Equal(postagemAtualizacao.Conteudo, response.Conteudo);
        Assert.Equal(postagemAtualizacao.UsuarioId, response.UsuarioId);
        Assert.Equal(postagemAtualizacao.TemaId, response.TemaId);
        Assert.NotNull(response.DataAtualizacao);
    }

    [Fact]
    public async Task AtualizarAsync_DeveRetornarNullQuandoPostagemNaoExiste()
    {
        var repository = new FakePostagemRepository();
        repository.AdicionarUsuario(1);
        repository.AdicionarTema(1);
        var service = new PostagemService(repository);
        var postagemAtualizacao = new PostagemAtualizacao
        {
            Titulo = "Titulo novo",
            Conteudo = "Conteudo novo",
            UsuarioId = 1,
            TemaId = 1
        };

        var response = await service.AtualizarAsync(99, postagemAtualizacao);

        Assert.Null(response);
    }

    [Fact]
    public async Task ExcluirAsync_DeveExcluirPostagemQuandoExiste()
    {
        var repository = new FakePostagemRepository();
        var postagem = repository.AdicionarPostagem(new Postagem
        {
            Titulo = "Postagem",
            Conteudo = "Conteudo",
            UsuarioId = 1,
            TemaId = 1
        });
        var service = new PostagemService(repository);

        var excluido = await service.ExcluirAsync(postagem.Id);

        Assert.True(excluido);
        Assert.False(repository.Existe(postagem.Id));
    }

    [Fact]
    public async Task ExcluirAsync_DeveRetornarFalseQuandoPostagemNaoExiste()
    {
        var repository = new FakePostagemRepository();
        var service = new PostagemService(repository);

        var excluido = await service.ExcluirAsync(99);

        Assert.False(excluido);
    }

    private static FakePostagemRepository CriarRepositoryComPostagens()
    {
        var repository = new FakePostagemRepository();
        repository.AdicionarPostagem(new Postagem
        {
            Titulo = "Postagem 1",
            Conteudo = "Conteudo 1",
            UsuarioId = 1,
            TemaId = 1
        });
        repository.AdicionarPostagem(new Postagem
        {
            Titulo = "Postagem 2",
            Conteudo = "Conteudo 2",
            UsuarioId = 1,
            TemaId = 2
        });
        repository.AdicionarPostagem(new Postagem
        {
            Titulo = "Postagem 3",
            Conteudo = "Conteudo 3",
            UsuarioId = 2,
            TemaId = 1
        });

        return repository;
    }

    private sealed class FakePostagemRepository : IPostagemRepository
    {
        private readonly List<Postagem> postagens = [];
        private readonly HashSet<long> usuarios = [];
        private readonly HashSet<long> temas = [];
        private long proximoId = 1;

        public Task<IReadOnlyList<Postagem>> ListarTodasAsync()
        {
            return Task.FromResult<IReadOnlyList<Postagem>>(postagens.OrderBy(postagem => postagem.Id).ToList());
        }

        public Task<IReadOnlyList<Postagem>> FiltrarAsync(long? autorId, long? temaId)
        {
            var query = postagens.AsEnumerable();

            if (autorId.HasValue)
            {
                query = query.Where(postagem => postagem.UsuarioId == autorId.Value);
            }

            if (temaId.HasValue)
            {
                query = query.Where(postagem => postagem.TemaId == temaId.Value);
            }

            return Task.FromResult<IReadOnlyList<Postagem>>(query.OrderBy(postagem => postagem.Id).ToList());
        }

        public Task<Postagem> CadastrarAsync(Postagem postagem)
        {
            AdicionarPostagem(postagem);

            return Task.FromResult(postagem);
        }

        public Task<Postagem?> AtualizarAsync(long id, Postagem dadosPostagem)
        {
            var postagem = postagens.FirstOrDefault(postagem => postagem.Id == id);

            if (postagem is null)
            {
                return Task.FromResult<Postagem?>(null);
            }

            postagem.Titulo = dadosPostagem.Titulo;
            postagem.Conteudo = dadosPostagem.Conteudo;
            postagem.UsuarioId = dadosPostagem.UsuarioId;
            postagem.TemaId = dadosPostagem.TemaId;
            postagem.DataAtualizacao = DateTime.UtcNow;

            return Task.FromResult<Postagem?>(postagem);
        }

        public Task<bool> ExcluirAsync(long id)
        {
            var postagem = postagens.FirstOrDefault(postagem => postagem.Id == id);

            if (postagem is null)
            {
                return Task.FromResult(false);
            }

            postagens.Remove(postagem);

            return Task.FromResult(true);
        }

        public Task<bool> UsuarioExisteAsync(long usuarioId)
        {
            return Task.FromResult(usuarios.Contains(usuarioId));
        }

        public Task<bool> TemaExisteAsync(long temaId)
        {
            return Task.FromResult(temas.Contains(temaId));
        }

        public Postagem AdicionarPostagem(Postagem postagem)
        {
            postagem.Id = postagem.Id == 0 ? proximoId++ : postagem.Id;
            postagens.Add(postagem);

            return postagem;
        }

        public void AdicionarUsuario(long id)
        {
            usuarios.Add(id);
        }

        public void AdicionarTema(long id)
        {
            temas.Add(id);
        }

        public bool Existe(long id)
        {
            return postagens.Any(postagem => postagem.Id == id);
        }
    }
}
