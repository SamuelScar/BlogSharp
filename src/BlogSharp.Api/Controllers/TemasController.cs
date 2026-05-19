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
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TemaResponse>>> ListarTodos()
    {
        var temas = await temaService.ListarTodosAsync();

        return Ok(temas);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<TemaResponse>> Cadastrar(TemaCadastro temaCadastro)
    {
        var tema = await temaService.CadastrarAsync(temaCadastro);

        return StatusCode(StatusCodes.Status201Created, tema);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:long}")]
    public async Task<ActionResult<TemaResponse>> Atualizar(long id, TemaAtualizacao temaAtualizacao)
    {
        var tema = await temaService.AtualizarAsync(id, temaAtualizacao);

        if (tema is null)
        {
            throw new RecursoNaoEncontradoException("Tema nao encontrado.");
        }

        return Ok(tema);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:long}")]
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
