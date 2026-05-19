using BlogSharp.Api.DTOs;
using BlogSharp.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace BlogSharp.Api.Controllers;

[ApiController]
[Route("api/postagens")]
public class PostagensController(IPostagemService postagemService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<PostagemResponse>>> ListarTodas()
    {
        var postagens = await postagemService.ListarTodasAsync();

        return Ok(postagens);
    }

    [HttpGet("filtro")]
    public async Task<ActionResult<IReadOnlyList<PostagemResponse>>> Filtrar([FromQuery] PostagemFiltro filtro)
    {
        var postagens = await postagemService.FiltrarAsync(filtro);

        return Ok(postagens);
    }

    [HttpPost]
    public async Task<ActionResult<PostagemResponse>> Cadastrar(PostagemCadastro postagemCadastro)
    {
        try
        {
            var postagem = await postagemService.CadastrarAsync(postagemCadastro);

            return StatusCode(StatusCodes.Status201Created, postagem);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { mensagem = ex.Message });
        }
    }

    [HttpPut("{id:long}")]
    public async Task<ActionResult<PostagemResponse>> Atualizar(long id, PostagemAtualizacao postagemAtualizacao)
    {
        try
        {
            var postagem = await postagemService.AtualizarAsync(id, postagemAtualizacao);

            return postagem is null ? NotFound(new { mensagem = "Postagem nao encontrada." }) : Ok(postagem);
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { mensagem = ex.Message });
        }
    }

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Excluir(long id)
    {
        var excluido = await postagemService.ExcluirAsync(id);

        return excluido ? NoContent() : NotFound(new { mensagem = "Postagem nao encontrada." });
    }
}
