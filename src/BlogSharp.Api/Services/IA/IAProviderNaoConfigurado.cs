using BlogSharp.Api.DTOs;
using BlogSharp.Api.Exceptions;

namespace BlogSharp.Api.Services.IA;

/// <summary>
/// Provider usado quando a integracao com IA esta desabilitada ou incompleta.
/// </summary>
public class IAProviderNaoConfigurado : IIAProvider
{
    public Task<ResultadoIA> GerarResumoAsync(string conteudo)
    {
        throw new IntegracaoIAException("Provedor de IA ainda nao configurado.");
    }
}
