using System.Net;
using System.Text;
using BlogSharp.Api.Exceptions;
using BlogSharp.Api.Services.IA;
using Microsoft.Extensions.Options;
using Xunit;

namespace BlogSharp.Api.Tests.Services;

public class OpenRouterIAProviderTests
{
    [Fact]
    public async Task GerarResumoAsync_DeveRetornarResultadoQuandoOpenRouterRetornaConteudoValido()
    {
        var handler = new FakeHttpMessageHandler(CriarRespostaOpenRouter(
            """{"resumo":"Resumo gerado.","tags":"API, REST, IA","categoria":"Tecnologia"}"""));
        var provider = CriarProvider(handler);

        var resultado = await provider.GerarResumoAsync("Conteudo da postagem");

        Assert.Equal("Resumo gerado.", resultado.Resumo);
        Assert.Equal("API, REST, IA", resultado.Tags);
        Assert.Equal("Tecnologia", resultado.Categoria);
        Assert.Equal(HttpMethod.Post, handler.Request!.Method);
        Assert.Equal("https://openrouter.ai/api/v1/chat/completions", handler.Request.RequestUri!.ToString());
        Assert.Equal("Bearer chave-teste", handler.Request.Headers.Authorization!.ToString());
        Assert.Contains("BlogSharp", handler.Request.Headers.GetValues("X-OpenRouter-Title"));
    }

    [Fact]
    public async Task GerarResumoAsync_DeveRecusarQuandoChaveNaoEstaConfigurada()
    {
        var handler = new FakeHttpMessageHandler(CriarRespostaOpenRouter(
            """{"resumo":"Resumo.","tags":"API, IA","categoria":"Tecnologia"}"""));
        var provider = CriarProvider(handler, apiKey: "");

        var exception = await Assert.ThrowsAsync<IntegracaoIAException>(
            () => provider.GerarResumoAsync("Conteudo da postagem"));

        Assert.Equal("Chave da IA nao configurada.", exception.Message);
        Assert.Equal(0, handler.RequestsEnviadas);
    }

    [Fact]
    public async Task GerarResumoAsync_DeveRecusarQuandoModeloNaoEstaConfigurado()
    {
        var handler = new FakeHttpMessageHandler(CriarRespostaOpenRouter(
            """{"resumo":"Resumo.","tags":"API, IA","categoria":"Tecnologia"}"""));
        var provider = CriarProvider(handler, model: "");

        var exception = await Assert.ThrowsAsync<IntegracaoIAException>(
            () => provider.GerarResumoAsync("Conteudo da postagem"));

        Assert.Equal("Modelo de IA nao configurado.", exception.Message);
        Assert.Equal(0, handler.RequestsEnviadas);
    }

    [Fact]
    public async Task GerarResumoAsync_DeveRecusarQuandoOpenRouterRetornaErroHttp()
    {
        var handler = new FakeHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.Forbidden)
        {
            Content = new StringContent("""{"error":"invalid key"}""", Encoding.UTF8, "application/json")
        });
        var provider = CriarProvider(handler);

        var exception = await Assert.ThrowsAsync<IntegracaoIAException>(
            () => provider.GerarResumoAsync("Conteudo da postagem"));

        Assert.Equal("Nao foi possivel gerar o resumo com IA no momento.", exception.Message);
        Assert.IsType<HttpRequestException>(exception.InnerException);
    }

    [Fact]
    public async Task GerarResumoAsync_DeveRecusarQuandoRespostaNaoTemChoices()
    {
        var handler = new FakeHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent("{}", Encoding.UTF8, "application/json")
        });
        var provider = CriarProvider(handler);

        var exception = await Assert.ThrowsAsync<IntegracaoIAException>(
            () => provider.GerarResumoAsync("Conteudo da postagem"));

        Assert.Equal("A IA nao retornou conteudo para a postagem.", exception.Message);
    }

    [Fact]
    public async Task GerarResumoAsync_DeveRecusarQuandoConteudoNaoEhJsonValido()
    {
        var handler = new FakeHttpMessageHandler(CriarRespostaOpenRouter("{"));
        var provider = CriarProvider(handler);

        var exception = await Assert.ThrowsAsync<IntegracaoIAException>(
            () => provider.GerarResumoAsync("Conteudo da postagem"));

        Assert.Equal("A IA retornou uma resposta invalida para a postagem.", exception.Message);
    }

    [Fact]
    public async Task GerarResumoAsync_DeveRecusarQuandoRespostaEstaIncompleta()
    {
        var handler = new FakeHttpMessageHandler(CriarRespostaOpenRouter(
            """{"resumo":"Resumo gerado.","tags":"","categoria":"Tecnologia"}"""));
        var provider = CriarProvider(handler);

        var exception = await Assert.ThrowsAsync<IntegracaoIAException>(
            () => provider.GerarResumoAsync("Conteudo da postagem"));

        Assert.Equal("A IA retornou uma resposta incompleta para a postagem.", exception.Message);
    }

    private static OpenRouterIAProvider CriarProvider(
        FakeHttpMessageHandler handler,
        string apiKey = "chave-teste",
        string model = "openai/gpt-5.2")
    {
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://openrouter.ai")
        };
        var options = Options.Create(new IAOptions
        {
            ApiKey = apiKey,
            Model = model,
            AppName = "BlogSharp"
        });

        return new OpenRouterIAProvider(httpClient, options);
    }

    private static HttpResponseMessage CriarRespostaOpenRouter(string content)
    {
        var responseBody = $$"""
            {
              "choices": [
                {
                  "message": {
                    "content": {{System.Text.Json.JsonSerializer.Serialize(content)}}
                  }
                }
              ]
            }
            """;

        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(responseBody, Encoding.UTF8, "application/json")
        };
    }

    private sealed class FakeHttpMessageHandler(HttpResponseMessage response) : HttpMessageHandler
    {
        public HttpRequestMessage? Request { get; private set; }

        public int RequestsEnviadas { get; private set; }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            Request = request;
            RequestsEnviadas++;

            return Task.FromResult(response);
        }
    }
}
