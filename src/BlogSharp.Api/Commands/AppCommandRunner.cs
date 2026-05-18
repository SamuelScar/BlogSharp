using BlogSharp.Api.Data.Seeders;

namespace BlogSharp.Api.Commands;

public static class AppCommandRunner
{
    public static async Task<bool> ExecutarAsync(string[] args, IServiceProvider services)
    {
        if (args.Length == 0 || !args[0].Equals("seed", StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }

        await ExecutarSeedAsync(args, services);

        return true;
    }

    private static async Task ExecutarSeedAsync(string[] args, IServiceProvider services)
    {
        if (args.Length != 3 ||
            !args[1].Equals("usuarios", StringComparison.OrdinalIgnoreCase) ||
            !int.TryParse(args[2], out var quantidade))
        {
            Console.WriteLine("Uso: dotnet run -- seed usuarios <quantidade>");
            return;
        }

        var registrosCriados = await services.SeedUsuariosAleatoriosAsync(quantidade);

        Console.WriteLine($"{registrosCriados} usuarios gerados com sucesso.");
        Console.WriteLine("Senha padrao dos usuarios gerados: Senha@123");
    }
}
