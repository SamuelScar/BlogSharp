using BlogSharp.Api.DTOs;

namespace BlogSharp.Api.Services.IA;

public interface IIAProvider
{
    Task<ResultadoIA> GerarResumoAsync(string conteudo);
}
