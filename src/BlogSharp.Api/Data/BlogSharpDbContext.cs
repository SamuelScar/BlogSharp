using BlogSharp.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BlogSharp.Api.Data;

public class BlogSharpDbContext(DbContextOptions<BlogSharpDbContext> options) : DbContext(options)
{
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Tema> Temas => Set<Tema>();
    public DbSet<Postagem> Postagens => Set<Postagem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Usuario>(entity => { entity.HasIndex(usuario => usuario.Email).IsUnique(); });

        modelBuilder.Entity<Postagem>()
            .HasOne(postagem => postagem.Usuario)
            .WithMany()
            .HasForeignKey(postagem => postagem.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Postagem>()
            .HasOne(postagem => postagem.Tema)
            .WithMany()
            .HasForeignKey(postagem => postagem.TemaId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
