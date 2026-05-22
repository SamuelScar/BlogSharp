using BlogSharp.Api.Commands;
using BlogSharp.Api.Config;
using BlogSharp.Api.Middlewares;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDatabase(builder.Configuration);
builder.Services.AddRepositories();
builder.Services.AddApplicationServices();
builder.Services.AddIA(builder.Configuration);
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

await app.SeedDevelopmentDataAsync();

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

await app.RunAsync();

// Necessario para os testes de integracao com WebApplicationFactory.
public partial class Program
{
}
