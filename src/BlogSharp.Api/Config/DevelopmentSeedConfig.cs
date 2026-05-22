using BlogSharp.Api.Data.Seeders;

namespace BlogSharp.Api.Config;

public static class DevelopmentSeedConfig
{
    public static async Task SeedDevelopmentDataAsync(this WebApplication app)
    {
        if (!app.Environment.IsDevelopment())
        {
            return;
        }

        await app.Services.SeedUsuariosAsync();
        await app.Services.SeedTemasAsync();
        await app.Services.SeedPostagensAsync();
    }
}
