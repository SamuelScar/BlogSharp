using BlogSharp.Api.DTOs;
using BlogSharp.Api.Security;
using BlogSharp.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlogSharp.Api.Controllers;

[ApiController]
[Route("api/usuarios")]
public class UsuariosController(IUsuarioService usuarioService) : ControllerBase
{
    [HttpPost("cadastrar")]
    public async Task<ActionResult<UsuarioResponse>> Cadastrar(UsuarioCadastro usuarioCadastro)
    {
        try
        {
            var usuario = await usuarioService.CadastrarAsync(usuarioCadastro);

            return StatusCode(StatusCodes.Status201Created, usuario);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { mensagem = ex.Message });
        }
    }

    [Authorize]
    [HttpPut("{id:long}")]
    public async Task<ActionResult<UsuarioResponse>> Atualizar(long id, UsuarioAtualizacao usuarioAtualizacao)
    {
        if (!UsuarioEhDono(id))
        {
            return Forbid();
        }

        try
        {
            var usuario = await usuarioService.AtualizarAsync(id, usuarioAtualizacao);

            return usuario is null ? NotFound(new { mensagem = "Usuario nao encontrado." }) : Ok(usuario);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { mensagem = ex.Message });
        }
    }

    [Authorize]
    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Excluir(long id)
    {
        if (!PodeExcluirUsuario(id))
        {
            return Forbid();
        }

        try
        {
            var excluido = await usuarioService.ExcluirAsync(id);

            return excluido ? NoContent() : NotFound(new { mensagem = "Usuario nao encontrado." });
        }
        catch (DbUpdateException)
        {
            return Conflict(new { mensagem = "Usuario possui postagens vinculadas." });
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<UsuarioLoginResponse>> Login(UsuarioLogin usuarioLogin)
    {
        var usuarioAutenticado = await usuarioService.AutenticarAsync(usuarioLogin);

        return usuarioAutenticado is null
            ? Unauthorized(new { mensagem = "Email ou senha invalidos." })
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
