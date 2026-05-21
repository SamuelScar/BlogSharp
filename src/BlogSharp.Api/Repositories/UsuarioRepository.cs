using BlogSharp.Api.Data;
using BlogSharp.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BlogSharp.Api.Repositories;

public class UsuarioRepository(BlogSharpDbContext context) : IUsuarioRepository
{
    public Task<Usuario?> BuscarPorEmailAsync(string email)
    {
        return context.Usuarios.FirstOrDefaultAsync(usuario => usuario.Email == email);
    }

    public async Task<Usuario> CadastrarAsync(Usuario usuario)
    {
        context.Usuarios.Add(usuario);
        await context.SaveChangesAsync();

        return usuario;
    }

    public async Task<Usuario?> AtualizarAsync(long id, Usuario usuarioAtualizado, bool atualizarSenha)
    {
        var usuario = await context.Usuarios.FirstOrDefaultAsync(usuario => usuario.Id == id);

        if (usuario is null)
        {
            return null;
        }

        usuario.Nome = usuarioAtualizado.Nome;
        usuario.Email = usuarioAtualizado.Email;
        usuario.Foto = usuarioAtualizado.Foto;

        if (atualizarSenha)
        {
            usuario.SenhaHash = usuarioAtualizado.SenhaHash;
        }

        await context.SaveChangesAsync();

        return usuario;
    }

    public async Task<Usuario?> AtualizarTipoAsync(long id, string tipo)
    {
        var usuario = await context.Usuarios.FirstOrDefaultAsync(usuario => usuario.Id == id);

        if (usuario is null)
        {
            return null;
        }

        usuario.Tipo = tipo;

        await context.SaveChangesAsync();

        return usuario;
    }

    public async Task<bool> ExcluirAsync(long id)
    {
        var registrosExcluidos = await context.Usuarios
            .Where(usuario => usuario.Id == id)
            .ExecuteDeleteAsync();

        return registrosExcluidos > 0;
    }
}
