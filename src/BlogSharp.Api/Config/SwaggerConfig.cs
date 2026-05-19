using BlogSharp.Api.Swagger;
using Microsoft.OpenApi.Models;

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

            options.OperationFilter<AuthorizeOperationFilter>();
        });

        return services;
    }
}
