using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using BlogSharp.Api.DTOs;
using BlogSharp.Api.Exceptions;
using Microsoft.Extensions.Options;

namespace BlogSharp.Api.Services.IA;

public class OpenRouterIAProvider(
    HttpClient httpClient,
    IOptions<IAOptions> options) : IIAProvider
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public async Task<ResultadoIA> GerarResumoAsync(string conteudo)
    {
        var iaOptions = options.Value;

        ValidarConfiguracao(iaOptions);

        using var request = new HttpRequestMessage(HttpMethod.Post, "api/v1/chat/completions")
        {
            Content = JsonContent.Create(CriarRequest(iaOptions.Model, conteudo), options: JsonOptions)
        };

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", iaOptions.ApiKey);
        AdicionarHeadersOpcionais(request, iaOptions);

        using var response = await httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            var detalheErro = await response.Content.ReadAsStringAsync();
            var exception = new HttpRequestException(
                $"OpenRouter retornou {(int)response.StatusCode} ({response.StatusCode}). {detalheErro}");

            throw new IntegracaoIAException("Nao foi possivel gerar o resumo com IA no momento.", exception);
        }

        var json = await ExtrairConteudoAsync(response);

        if (string.IsNullOrWhiteSpace(json))
        {
            throw new IntegracaoIAException("A IA nao retornou conteudo para a postagem.");
        }

        try
        {
            return JsonSerializer.Deserialize<ResultadoIA>(json, JsonOptions)
                ?? throw new IntegracaoIAException("A IA retornou uma resposta invalida para a postagem.");
        }
        catch (JsonException exception)
        {
            throw new IntegracaoIAException("A IA retornou uma resposta invalida para a postagem.", exception);
        }
    }

    private static void ValidarConfiguracao(IAOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.ApiKey))
        {
            throw new IntegracaoIAException("Chave da IA nao configurada.");
        }

        if (string.IsNullOrWhiteSpace(options.Model))
        {
            throw new IntegracaoIAException("Modelo de IA nao configurado.");
        }
    }

    private static void AdicionarHeadersOpcionais(HttpRequestMessage request, IAOptions options)
    {
        if (!string.IsNullOrWhiteSpace(options.SiteUrl))
        {
            request.Headers.Add("HTTP-Referer", options.SiteUrl);
        }

        if (!string.IsNullOrWhiteSpace(options.AppName))
        {
            request.Headers.Add("X-OpenRouter-Title", options.AppName);
        }
    }

    private static OpenRouterRequest CriarRequest(string model, string conteudo)
    {
        return new OpenRouterRequest
        {
            Model = model,
            Temperature = 0.7,
            MaxTokens = 300,
            Messages =
            [
                new OpenRouterMessage
                {
                    Role = "system",
                    Content = "Voce responde somente JSON valido, sem Markdown e sem texto adicional."
                },
                new OpenRouterMessage
                {
                    Role = "user",
                    Content = PromptBuilder.CriarPromptResumoPostagem(conteudo)
                }
            ]
        };
    }

    private static async Task<string?> ExtrairConteudoAsync(HttpResponseMessage response)
    {
        await using var stream = await response.Content.ReadAsStreamAsync();
        using var json = await JsonDocument.ParseAsync(stream);

        if (!json.RootElement.TryGetProperty("choices", out var choices)
            || choices.ValueKind != JsonValueKind.Array
            || choices.GetArrayLength() == 0)
        {
            return null;
        }

        var primeiraEscolha = choices[0];

        if (!primeiraEscolha.TryGetProperty("message", out var message)
            || !message.TryGetProperty("content", out var content))
        {
            return null;
        }

        return content.GetString();
    }

    private sealed class OpenRouterRequest
    {
        public string Model { get; set; } = string.Empty;

        public List<OpenRouterMessage> Messages { get; set; } = [];

        public double Temperature { get; set; }

        [JsonPropertyName("max_tokens")]
        public int MaxTokens { get; set; }
    }

    private sealed class OpenRouterMessage
    {
        public string Role { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;
    }

}
