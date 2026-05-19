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

    public async Task<bool> AtualizarAsync(long id, Usuario usuario, bool atualizarSenha)
    {
        usuario.Id = id;
        context.Usuarios.Attach(usuario);

        var entrada = context.Entry(usuario);
        entrada.Property(usuarioBanco => usuarioBanco.Nome).IsModified = true;
        entrada.Property(usuarioBanco => usuarioBanco.Email).IsModified = true;
        entrada.Property(usuarioBanco => usuarioBanco.Tipo).IsModified = true;
        entrada.Property(usuarioBanco => usuarioBanco.Foto).IsModified = true;

        if (atualizarSenha)
        {
            entrada.Property(usuarioBanco => usuarioBanco.SenhaHash).IsModified = true;
        }

        try
        {
            await context.SaveChangesAsync();
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            return false;
        }
    }

    public async Task<bool> ExcluirAsync(long id)
    {
        var registrosExcluidos = await context.Usuarios
            .Where(usuario => usuario.Id == id)
            .ExecuteDeleteAsync();

        return registrosExcluidos > 0;
    }
}
