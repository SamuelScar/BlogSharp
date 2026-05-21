using BlogSharp.Api.DTOs;
using BlogSharp.Api.Services.IA;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogSharp.Api.Controllers;

[ApiController]
[Authorize]
[Route("api/ia")]
public class IAController(IIAService iaService) : ControllerBase
{
    /// <summary>
    /// Gera resumo, categoria e tags para o conteudo informado.
    /// </summary>
    [HttpPost("resumir")]
    [ProducesResponseType(typeof(ResultadoIA), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status502BadGateway)]
    public async Task<ActionResult<ResultadoIA>> Resumir(ResumoIARequest request)
    {
        var resultado = await iaService.GerarResumoAsync(request.Conteudo);

        return Ok(resultado);
    }
}
