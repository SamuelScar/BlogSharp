using BlogSharp.Api.DTOs;
using BlogSharp.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

        return tema is null ? NotFound(new { mensagem = "Tema nao encontrado." }) : Ok(tema);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Excluir(long id)
    {
        try
        {
            var excluido = await temaService.ExcluirAsync(id);

            return excluido ? NoContent() : NotFound(new { mensagem = "Tema nao encontrado." });
        }
        catch (DbUpdateException)
        {
            return Conflict(new { mensagem = "Tema possui postagens vinculadas." });
        }
    }
}
