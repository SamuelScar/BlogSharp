using BlogSharp.Api.DTOs;
using BlogSharp.Api.Exceptions;
using BlogSharp.Api.Models;
using BlogSharp.Api.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BlogSharp.Api.Services;

public class UsuarioService(
    IUsuarioRepository usuarioRepository,
    IPasswordHasher<Usuario> passwordHasher,
    ITokenService tokenService) : IUsuarioService
{
    private const string TipoAdmin = "Admin";
    private const string TipoUsuario = "Usuario";

    public async Task<UsuarioResponse> CadastrarAsync(UsuarioCadastro usuarioCadastro)
    {
        var emailCadastrado = await usuarioRepository.BuscarPorEmailAsync(usuarioCadastro.Email);

        if (emailCadastrado is not null)
        {
            throw new ConflitoException("Email ja cadastrado.");
        }

        var usuario = new Usuario
        {
            Nome = usuarioCadastro.Nome,
            Email = usuarioCadastro.Email,
            Tipo = TipoUsuario,
            Foto = usuarioCadastro.Foto
        };

        usuario.SenhaHash = passwordHasher.HashPassword(usuario, usuarioCadastro.Senha);

        try
        {
            var usuarioCadastrado = await usuarioRepository.CadastrarAsync(usuario);

            return MapearResponse(usuarioCadastrado);
        }
        catch (DbUpdateException ex)
        {
            throw new ConflitoException("Email ja cadastrado.", ex);
        }
    }

    public async Task<UsuarioResponse?> AtualizarAsync(long id, UsuarioAtualizacao usuarioAtualizacao)
    {
        var usuario = new Usuario
        {
            Id = id,
            Nome = usuarioAtualizacao.Nome,
            Email = usuarioAtualizacao.Email,
            Foto = usuarioAtualizacao.Foto
        };
        var atualizarSenha = !string.IsNullOrWhiteSpace(usuarioAtualizacao.Senha);

        if (atualizarSenha)
        {
            usuario.SenhaHash = passwordHasher.HashPassword(usuario, usuarioAtualizacao.Senha!);
        }

        try
        {
            var usuarioAtualizado = await usuarioRepository.AtualizarAsync(id, usuario, atualizarSenha);

            return usuarioAtualizado is null ? null : MapearResponse(usuarioAtualizado);
        }
        catch (DbUpdateException ex)
        {
            throw new ConflitoException("Email ja cadastrado.", ex);
        }
    }

    public async Task<UsuarioResponse?> AtualizarPrivilegioAsync(
        long usuarioAutenticadoId,
        long id,
        UsuarioPrivilegioAtualizacao usuarioPrivilegioAtualizacao)
    {
        if (usuarioAutenticadoId == id)
        {
            throw new AcessoNegadoException("Administrador nao pode alterar o proprio privilegio.");
        }

        var tipo = ValidarTipo(usuarioPrivilegioAtualizacao.Tipo);
        var usuario = await usuarioRepository.AtualizarTipoAsync(id, tipo);

        return usuario is null ? null : MapearResponse(usuario);
    }

    public async Task<bool> ExcluirAsync(long id)
    {
        try
        {
            return await usuarioRepository.ExcluirAsync(id);
        }
        catch (DbUpdateException ex)
        {
            throw new ConflitoException("Usuario possui postagens vinculadas.", ex);
        }
    }

    public async Task<UsuarioLoginResponse?> AutenticarAsync(UsuarioLogin usuarioLogin)
    {
        var usuario = await usuarioRepository.BuscarPorEmailAsync(usuarioLogin.Email);

        if (usuario is null)
        {
            return null;
        }

        var resultado = passwordHasher.VerifyHashedPassword(usuario, usuario.SenhaHash, usuarioLogin.Senha);

        if (resultado == PasswordVerificationResult.Failed)
        {
            return null;
        }

        return new UsuarioLoginResponse
        {
            Id = usuario.Id,
            Nome = usuario.Nome,
            Email = usuario.Email,
            Tipo = usuario.Tipo,
            Foto = usuario.Foto,
            Token = tokenService.GerarToken(usuario)
        };
    }

    private static UsuarioResponse MapearResponse(Usuario usuario)
    {
        return new UsuarioResponse
        {
            Id = usuario.Id,
            Nome = usuario.Nome,
            Email = usuario.Email,
            Tipo = usuario.Tipo,
            Foto = usuario.Foto
        };
    }

    private static string ValidarTipo(string tipo)
    {
        return tipo?.Trim() switch
        {
            TipoUsuario => TipoUsuario,
            TipoAdmin => TipoAdmin,
            _ => throw new RequisicaoInvalidaException("Tipo de usuario invalido. Use Usuario ou Admin.")
        };
    }
}
