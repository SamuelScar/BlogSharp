using BlogSharp.Api.DTOs;
using BlogSharp.Api.Exceptions;
using BlogSharp.Api.Security;
using BlogSharp.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogSharp.Api.Controllers;

[ApiController]
[Route("api/usuarios")]
public class UsuariosController(IUsuarioService usuarioService) : ControllerBase
{
    [HttpPost("cadastrar")]
    public async Task<ActionResult<UsuarioResponse>> Cadastrar(UsuarioCadastro usuarioCadastro)
    {
        var usuario = await usuarioService.CadastrarAsync(usuarioCadastro);

        return StatusCode(StatusCodes.Status201Created, usuario);
    }

    [Authorize]
    [HttpPut("{id:long}")]
    public async Task<ActionResult<UsuarioResponse>> Atualizar(long id, UsuarioAtualizacao usuarioAtualizacao)
    {
        if (!UsuarioEhDono(id))
        {
            return Forbid();
        }

        var usuario = await usuarioService.AtualizarAsync(id, usuarioAtualizacao);

        if (usuario is null)
        {
            throw new RecursoNaoEncontradoException("Usuario nao encontrado.");
        }

        return Ok(usuario);
    }

    [Authorize]
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Excluir(long id)
    {
        if (!PodeExcluirUsuario(id))
        {
            return Forbid();
        }

        var excluido = await usuarioService.ExcluirAsync(id);

        if (!excluido)
        {
            throw new RecursoNaoEncontradoException("Usuario nao encontrado.");
        }

        return NoContent();
    }

    [HttpPost("login")]
    public async Task<ActionResult<UsuarioLoginResponse>> Login(UsuarioLogin usuarioLogin)
    {
        var usuarioAutenticado = await usuarioService.AutenticarAsync(usuarioLogin);

        return usuarioAutenticado is null
            ? Unauthorized(new ErroResponse("Email ou senha invalidos."))
            : Ok(usuarioAutenticado);
    }

    private bool UsuarioEhDono(long id)
    {
        return this.ObterUsuarioId() == id;
    }

    private bool PodeExcluirUsuario(long id)
    {
        return this.UsuarioEhAdmin() || this.ObterUsuarioId() == id;
    }
}
