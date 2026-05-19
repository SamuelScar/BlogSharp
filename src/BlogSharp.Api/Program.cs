using BlogSharp.Api.Commands;
using BlogSharp.Api.Config;
using BlogSharp.Api.Data;
using BlogSharp.Api.Data.Seeders;
using BlogSharp.Api.Middlewares;
using BlogSharp.Api.Models;
using BlogSharp.Api.Repositories;
using BlogSharp.Api.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<BlogSharpDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<ITemaRepository, TemaRepository>();
builder.Services.AddScoped<ITemaService, TemaService>();
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IPostagemRepository, PostagemRepository>();
builder.Services.AddScoped<IPostagemService, PostagemService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IPasswordHasher<Usuario>, PasswordHasher<Usuario>>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerComJwt();
builder.Services.AddAuthorization();
builder.Services.AddJwtAuthentication(builder.Configuration);

var app = builder.Build();

if (await AppCommandRunner.ExecutarAsync(args, app.Services))
{
    return;
}

if (app.Environment.IsDevelopment())
{
    await app.Services.SeedUsuariosAsync();
    await app.Services.SeedTemasAsync();
    await app.Services.SeedPostagensAsync();
}

app.UseMiddleware<ExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapGet("/", () => Results.Redirect("/swagger"));

app.UseJwtAuthentication(builder.Configuration);
app.UseAuthorization();

app.MapGet("/health", () => Results.Ok(new
{
    status = "ok",
    application = "BlogSharp.Api"
}));

app.MapControllers();

app.Run();
