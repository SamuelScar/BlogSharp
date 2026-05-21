using BlogSharp.Api.DTOs;
using BlogSharp.Api.Exceptions;
using Microsoft.Extensions.Options;

namespace BlogSharp.Api.Services.IA;

public class IAService(
    IIAProvider iaProvider,
    IOptions<IAOptions> options) : IIAService
{
    public async Task<ResultadoIA> GerarResumoAsync(string conteudo)
    {
        if (!options.Value.Enabled)
        {
            throw new IntegracaoIAException("Integracao com IA nao habilitada.");
        }

        return await iaProvider.GerarResumoAsync(conteudo);
    }
}
