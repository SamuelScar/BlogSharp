using BlogSharp.Api.Services.IA;

namespace BlogSharp.Api.Config;

public static class IAConfig
{
    public static IServiceCollection AddIA(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<IAOptions>(configuration.GetSection("IA"));

        services.AddHttpClient<OpenRouterIAProvider>(client =>
        {
            var configuredBaseUrl = configuration["IA:BaseUrl"];
            var baseUrl = string.IsNullOrWhiteSpace(configuredBaseUrl)
                ? "https://openrouter.ai"
                : configuredBaseUrl;

            client.BaseAddress = new Uri(baseUrl, UriKind.Absolute);
        });

        services.AddScoped<IAProviderNaoConfigurado>();
        services.AddScoped<IIAProvider>(serviceProvider =>
        {
            var provider = configuration["IA:Provider"];

            return provider?.Equals("OpenRouter", StringComparison.OrdinalIgnoreCase) == true
                ? serviceProvider.GetRequiredService<OpenRouterIAProvider>()
                : serviceProvider.GetRequiredService<IAProviderNaoConfigurado>();
        });
        services.AddScoped<IIAService, IAService>();

        return services;
    }
}
