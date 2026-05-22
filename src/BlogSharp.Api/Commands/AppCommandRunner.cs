using BlogSharp.Api.Data.Seeders;

namespace BlogSharp.Api.Commands;

public static class AppCommandRunner
{
    /// <summary>
    /// Executa comandos auxiliares da aplicacao, como a geracao manual de seeds.
    /// </summary>
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
        if (args.Length != 3 || !int.TryParse(args[2], out var quantidade))
        {
            ExibirUsoSeed();
            return;
        }

        if (args[1].Equals("usuarios", StringComparison.OrdinalIgnoreCase))
        {
            var registrosCriados = await services.SeedUsuariosAleatoriosAsync(quantidade);

            Console.WriteLine($"{registrosCriados} usuarios gerados com sucesso.");
            Console.WriteLine("Senha padrao dos usuarios gerados: Senha@123");
            return;
        }

        if (args[1].Equals("temas", StringComparison.OrdinalIgnoreCase))
        {
            var registrosCriados = await services.SeedTemasAleatoriosAsync(quantidade);

            Console.WriteLine($"{registrosCriados} temas gerados com sucesso.");
            return;
        }

        if (args[1].Equals("postagens", StringComparison.OrdinalIgnoreCase))
        {
            var registrosCriados = await services.SeedPostagensAleatoriasAsync(quantidade);

            Console.WriteLine($"{registrosCriados} postagens geradas com sucesso.");
            return;
        }

        ExibirUsoSeed();
    }

    private static void ExibirUsoSeed()
    {
        Console.WriteLine("Uso:");
        Console.WriteLine("  dotnet run -- seed usuarios <quantidade>");
        Console.WriteLine("  dotnet run -- seed temas <quantidade>");
        Console.WriteLine("  dotnet run -- seed postagens <quantidade>");
    }
}
