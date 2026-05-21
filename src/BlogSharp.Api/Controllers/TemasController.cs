using BlogSharp.Api.DTOs;
using BlogSharp.Api.Exceptions;
using BlogSharp.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogSharp.Api.Controllers;

[ApiController]
[Route("api/temas")]
public class TemasController(ITemaService temaService) : ControllerBase
{
    /// <summary>
    /// Lista todos os temas cadastrados.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TemaResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TemaResponse>>> ListarTodos()
    {
        var temas = await temaService.ListarTodosAsync();

        return Ok(temas);
    }

    /// <summary>
    /// Cadastra um novo tema. Acesso restrito a administradores.
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ProducesResponseType(typeof(TemaResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<TemaResponse>> Cadastrar(TemaCadastro temaCadastro)
    {
        var tema = await temaService.CadastrarAsync(temaCadastro);

        return StatusCode(StatusCodes.Status201Created, tema);
    }

    /// <summary>
    /// Atualiza um tema existente. Acesso restrito a administradores.
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPut("{id:long}")]
    [ProducesResponseType(typeof(TemaResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TemaResponse>> Atualizar(long id, TemaAtualizacao temaAtualizacao)
    {
        var tema = await temaService.AtualizarAsync(id, temaAtualizacao);

        if (tema is null)
        {
            throw new RecursoNaoEncontradoException("Tema nao encontrado.");
        }

        return Ok(tema);
    }

    /// <summary>
    /// Exclui um tema existente. Acesso restrito a administradores.
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Excluir(long id)
    {
        var excluido = await temaService.ExcluirAsync(id);

        if (!excluido)
        {
            throw new RecursoNaoEncontradoException("Tema nao encontrado.");
        }

        return NoContent();
    }
}
