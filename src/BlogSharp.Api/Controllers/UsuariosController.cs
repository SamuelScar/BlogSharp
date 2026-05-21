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
    /// <summary>
    /// Cadastra um novo usuario.
    /// </summary>
    [HttpPost("cadastrar")]
    [ProducesResponseType(typeof(UsuarioResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UsuarioResponse>> Cadastrar(UsuarioCadastro usuarioCadastro)
    {
        var usuario = await usuarioService.CadastrarAsync(usuarioCadastro);

        return StatusCode(StatusCodes.Status201Created, usuario);
    }

    /// <summary>
    /// Atualiza os dados do proprio usuario autenticado.
    /// </summary>
    [Authorize]
    [HttpPut("{id:long}")]
    [ProducesResponseType(typeof(UsuarioResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status409Conflict)]
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

    /// <summary>
    /// Atualiza o perfil de acesso de outro usuario. Acesso restrito a administradores.
    /// </summary>
    [Authorize(Roles = "Admin")]
    [HttpPatch("{id:long}/privilegio")]
    [ProducesResponseType(typeof(UsuarioResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UsuarioResponse>> AtualizarPrivilegio(
        long id,
        UsuarioPrivilegioAtualizacao usuarioPrivilegioAtualizacao)
    {
        if (UsuarioEhDono(id))
        {
            return Forbid();
        }

        var usuario = await usuarioService.AtualizarPrivilegioAsync(id, usuarioPrivilegioAtualizacao);

        if (usuario is null)
        {
            throw new RecursoNaoEncontradoException("Usuario nao encontrado.");
        }

        return Ok(usuario);
    }

    /// <summary>
    /// Exclui o proprio usuario autenticado ou qualquer usuario quando o perfil for administrador.
    /// </summary>
    [Authorize]
    [HttpDelete("{id:long}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status409Conflict)]
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

    /// <summary>
    /// Autentica o usuario com email e senha e retorna um token JWT.
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(UsuarioLoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ErroResponse), StatusCodes.Status401Unauthorized)]
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
