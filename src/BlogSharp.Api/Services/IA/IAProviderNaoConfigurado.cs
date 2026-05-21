using BlogSharp.Api.DTOs;
using BlogSharp.Api.Exceptions;

namespace BlogSharp.Api.Services.IA;

public class IAProviderNaoConfigurado : IIAProvider
{
    public Task<ResultadoIA> GerarResumoAsync(string _)
    {
        throw new IntegracaoIAException("Provedor de IA ainda nao configurado.");
    }
}
