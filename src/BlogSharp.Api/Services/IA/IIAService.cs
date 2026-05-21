using BlogSharp.Api.DTOs;

namespace BlogSharp.Api.Services.IA;

public interface IIAService
{
    Task<ResultadoIA> GerarResumoAsync(string conteudo);
}
