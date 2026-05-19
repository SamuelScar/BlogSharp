using BlogSharp.Api.DTOs;
using BlogSharp.Api.Security;
using BlogSharp.Api.Services;
using Microsoft.AspNetCore.Authorization;
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

    [Authorize]
    [HttpPost]
    public async Task<ActionResult<PostagemResponse>> Cadastrar(PostagemCadastro postagemCadastro)
    {
        var usuarioId = this.ObterUsuarioId();

        if (usuarioId is null)
        {
            return Unauthorized(new { mensagem = "Token invalido." });
        }

        if (postagemCadastro.UsuarioId != usuarioId)
        {
            return Forbid();
        }

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

    [Authorize]
    [HttpPut("{id:long}")]
    public async Task<ActionResult<PostagemResponse>> Atualizar(long id, PostagemAtualizacao postagemAtualizacao)
    {
        var usuarioId = this.ObterUsuarioId();

        if (usuarioId is null)
        {
            return Unauthorized(new { mensagem = "Token invalido." });
        }

        var donoId = await postagemService.BuscarUsuarioIdAsync(id);

        if (donoId is null)
        {
            return NotFound(new { mensagem = "Postagem nao encontrada." });
        }

        if (donoId != usuarioId || postagemAtualizacao.UsuarioId != usuarioId)
        {
            return Forbid();
        }

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

    [Authorize]
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Excluir(long id)
    {
        var usuarioId = this.ObterUsuarioId();

        if (usuarioId is null)
        {
            return Unauthorized(new { mensagem = "Token invalido." });
        }

        var donoId = await postagemService.BuscarUsuarioIdAsync(id);

        if (donoId is null)
        {
            return NotFound(new { mensagem = "Postagem nao encontrada." });
        }

        if (donoId != usuarioId && !this.UsuarioEhAdmin())
        {
            return Forbid();
        }

        var excluido = await postagemService.ExcluirAsync(id);

        return excluido ? NoContent() : NotFound(new { mensagem = "Postagem nao encontrada." });
    }
}
