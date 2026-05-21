using BlogSharp.Api.DTOs;
using BlogSharp.Api.Exceptions;
using BlogSharp.Api.Security;
using BlogSharp.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogSharp.Api.Controllers;

[ApiController]
[Route("api/postagens")]
public class PostagensController(IPostagemService postagemService) : ControllerBase
{
    private const string PostagemNaoEncontrada = "Postagem nao encontrada.";

    /// <summary>
    /// Lista todas as postagens cadastradas.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<PostagemResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PostagemResponse>>> ListarTodas()
    {
        var postagens = await postagemService.ListarTodasAsync();

        return Ok(postagens);
    }

    /// <summary>
    /// Filtra postagens por autor, tema ou pelos dois criterios.
    /// </summary>
    [HttpGet("filtro")]
    [ProducesResponseType(typeof(IReadOnlyList<PostagemResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<PostagemResponse>>> Filtrar([FromQuery] PostagemFiltro filtro)
    {
        var postagens = await postagemService.FiltrarAsync(filtro);

        return Ok(postagens);
    }

    /// <summary>
    /// Cria uma postagem vinculada ao usuario autenticado e a um tema existente.
    /// </summary>
    [Authorize]
    [HttpPost]
    [ProducesResponseType(typeof(PostagemResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PostagemResponse>> Cadastrar(PostagemCadastro postagemCadastro)
    {
        var usuarioId = this.ObterUsuarioId();

        if (usuarioId is null)
        {
            return Unauthorized(new ErroResponse("Token invalido."));
        }

        if (postagemCadastro.UsuarioId != usuarioId)
        {
            return Forbid();
        }

        var postagem = await postagemService.CadastrarAsync(postagemCadastro);

        return StatusCode(StatusCodes.Status201Created, postagem);
    }

    /// <summary>
    /// Atualiza uma postagem existente quando o usuario autenticado e o dono da postagem.
    /// </summary>
    [Authorize]
    [HttpPut("{id:long}")]
    [ProducesResponseType(typeof(PostagemResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PostagemResponse>> Atualizar(long id, PostagemAtualizacao postagemAtualizacao)
    {
        var usuarioId = this.ObterUsuarioId();

        if (usuarioId is null)
        {
            return Unauthorized(new ErroResponse("Token invalido."));
        }

        var donoId = await postagemService.BuscarUsuarioIdAsync(id);

        if (donoId is null)
        {
            throw new RecursoNaoEncontradoException(PostagemNaoEncontrada);
        }

        if (donoId != usuarioId || postagemAtualizacao.UsuarioId != usuarioId)
        {
            return Forbid();
        }

        var postagem = await postagemService.AtualizarAsync(id, postagemAtualizacao);

        if (postagem is null)
        {
            throw new RecursoNaoEncontradoException(PostagemNaoEncontrada);
        }

        return Ok(postagem);
    }

    /// <summary>
    /// Exclui uma postagem existente quando o usuario autenticado e o dono ou administrador.
    /// </summary>
    [Authorize]
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Excluir(long id)
    {
        var usuarioId = this.ObterUsuarioId();

        if (usuarioId is null)
        {
            return Unauthorized(new ErroResponse("Token invalido."));
        }

        var donoId = await postagemService.BuscarUsuarioIdAsync(id);

        if (donoId is null)
        {
            throw new RecursoNaoEncontradoException(PostagemNaoEncontrada);
        }

        if (donoId != usuarioId && !this.UsuarioEhAdmin())
        {
            return Forbid();
        }

        var excluido = await postagemService.ExcluirAsync(id);

        if (!excluido)
        {
            throw new RecursoNaoEncontradoException(PostagemNaoEncontrada);
        }

        return NoContent();
    }
}
