using BlogSharp.Api.Data;
using BlogSharp.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BlogSharp.Api.Repositories;

public class UsuarioRepository(BlogSharpDbContext context) : IUsuarioRepository
{
    public Task<Usuario?> BuscarPorIdAsync(long id)
    {
        return context.Usuarios.FirstOrDefaultAsync(usuario => usuario.Id == id);
    }

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

    public async Task<Usuario> AtualizarAsync(Usuario usuario)
    {
        context.Usuarios.Update(usuario);
        await context.SaveChangesAsync();

        return usuario;
    }

    public async Task ExcluirAsync(Usuario usuario)
    {
        context.Usuarios.Remove(usuario);
        await context.SaveChangesAsync();
    }
}
