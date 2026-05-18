using BlogSharp.Api.DTOs;
using BlogSharp.Api.Models;
using BlogSharp.Api.Repositories;
using Microsoft.AspNetCore.Identity;

namespace BlogSharp.Api.Services;

public class UsuarioService(
    IUsuarioRepository usuarioRepository,
    IPasswordHasher<Usuario> passwordHasher,
    ITokenService tokenService) : IUsuarioService
{
    public async Task<UsuarioResponse?> BuscarPorIdAsync(long id)
    {
        var usuario = await usuarioRepository.BuscarPorIdAsync(id);

        return usuario is null ? null : MapearResponse(usuario);
    }

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
        var usuario = await usuarioRepository.BuscarPorIdAsync(id);

        if (usuario is null)
        {
            return null;
        }

        var emailCadastrado = await usuarioRepository.BuscarPorEmailAsync(usuarioAtualizacao.Email);

        if (emailCadastrado is not null && emailCadastrado.Id != id)
        {
            throw new InvalidOperationException("Email ja cadastrado.");
        }

        usuario.Nome = usuarioAtualizacao.Nome;
        usuario.Email = usuarioAtualizacao.Email;
        usuario.Tipo = usuarioAtualizacao.Tipo;
        usuario.Foto = usuarioAtualizacao.Foto;

        if (!string.IsNullOrWhiteSpace(usuarioAtualizacao.Senha))
        {
            usuario.SenhaHash = passwordHasher.HashPassword(usuario, usuarioAtualizacao.Senha);
        }

        var usuarioAtualizado = await usuarioRepository.AtualizarAsync(usuario);

        return MapearResponse(usuarioAtualizado);
    }

    public async Task<bool> ExcluirAsync(long id)
    {
        var usuario = await usuarioRepository.BuscarPorIdAsync(id);

        if (usuario is null)
        {
            return false;
        }

        await usuarioRepository.ExcluirAsync(usuario);

        return true;
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
