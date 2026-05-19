using BlogSharp.Api.DTOs;
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
    public async Task<UsuarioResponse> CadastrarAsync(UsuarioCadastro usuarioCadastro)
    {
        var emailCadastrado = await usuarioRepository.BuscarPorEmailAsync(usuarioCadastro.Email);

        if (emailCadastrado is not null)
        {
            throw new InvalidOperationException("Email ja cadastrado.");
        }

        var usuario = new Usuario
        {
            Nome = usuarioCadastro.Nome,
            Email = usuarioCadastro.Email,
            Tipo = usuarioCadastro.Tipo,
            Foto = usuarioCadastro.Foto
        };

        usuario.SenhaHash = passwordHasher.HashPassword(usuario, usuarioCadastro.Senha);

        var usuarioCadastrado = await usuarioRepository.CadastrarAsync(usuario);

        return MapearResponse(usuarioCadastrado);
    }

    public async Task<UsuarioResponse?> AtualizarAsync(long id, UsuarioAtualizacao usuarioAtualizacao)
    {
        var usuario = new Usuario
        {
            Id = id,
            Nome = usuarioAtualizacao.Nome,
            Email = usuarioAtualizacao.Email,
            Tipo = usuarioAtualizacao.Tipo,
            Foto = usuarioAtualizacao.Foto
        };
        var atualizarSenha = !string.IsNullOrWhiteSpace(usuarioAtualizacao.Senha);

        if (atualizarSenha)
        {
            usuario.SenhaHash = passwordHasher.HashPassword(usuario, usuarioAtualizacao.Senha!);
        }

        try
        {
            var atualizado = await usuarioRepository.AtualizarAsync(id, usuario, atualizarSenha);

            return atualizado ? MapearResponse(usuario) : null;
        }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException("Email ja cadastrado.", ex);
        }
    }

    public async Task<bool> ExcluirAsync(long id)
    {
        return await usuarioRepository.ExcluirAsync(id);
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
}
