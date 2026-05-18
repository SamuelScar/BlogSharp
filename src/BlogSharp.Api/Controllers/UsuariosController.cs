using BlogSharp.Api.DTOs;
using BlogSharp.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BlogSharp.Api.Controllers;

[ApiController]
[Route("api/usuarios")]
public class UsuariosController(IUsuarioService usuarioService) : ControllerBase
{
    [HttpGet("{id:long}")]
    public async Task<ActionResult<UsuarioResponse>> BuscarPorId(long id)
    {
        var usuario = await usuarioService.BuscarPorIdAsync(id);

        return usuario is null ? NotFound(new { mensagem = "Usuario nao encontrado." }) : Ok(usuario);
    }

    [HttpPost("cadastrar")]
    public async Task<ActionResult<UsuarioResponse>> Cadastrar(UsuarioCadastro usuarioCadastro)
    {
        try
        {
            var usuario = await usuarioService.CadastrarAsync(usuarioCadastro);

            return CreatedAtAction(nameof(BuscarPorId), new { id = usuario.Id }, usuario);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { mensagem = ex.Message });
        }
    }

    [HttpPut("{id:long}")]
    public async Task<ActionResult<UsuarioResponse>> Atualizar(long id, UsuarioAtualizacao usuarioAtualizacao)
    {
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

    [HttpDelete("{id:long}")]
    public async Task<IActionResult> Excluir(long id)
    {
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
}
