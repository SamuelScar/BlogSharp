using BlogSharp.Api.Swagger;
using Microsoft.OpenApi.Models;
using System.Reflection;

namespace BlogSharp.Api.Config;

public static class SwaggerConfig
{
    public static IServiceCollection AddSwaggerComJwt(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "Cole apenas o token JWT retornado no login.",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT"
            });

            var xmlFileName = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlFilePath = Path.Combine(AppContext.BaseDirectory, xmlFileName);

            if (File.Exists(xmlFilePath))
            {
                options.IncludeXmlComments(xmlFilePath);
            }

            options.OperationFilter<AuthorizeOperationFilter>();
        });

        return services;
    }
}
