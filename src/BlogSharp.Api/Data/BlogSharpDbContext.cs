using BlogSharp.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace BlogSharp.Api.Data;

public class BlogSharpDbContext(DbContextOptions<BlogSharpDbContext> options) : DbContext(options)
{
    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Tema> Temas { get; set; }
    public DbSet<Postagem> Postagens { get; set; }

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
