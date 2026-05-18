using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace BlogSharp.Api.Config;

public static class JwtConfig
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtSecretKey = configuration["Jwt:SecretKey"];

        if (string.IsNullOrWhiteSpace(jwtSecretKey))
        {
            return services;
        }

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
                    ValidateIssuer = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidateAudience = true,
                    ValidAudience = configuration["Jwt:Audience"],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
            });

        return services;
    }

    public static IApplicationBuilder UseJwtAuthentication(this IApplicationBuilder app, IConfiguration configuration)
    {
        var jwtSecretKey = configuration["Jwt:SecretKey"];

        if (!string.IsNullOrWhiteSpace(jwtSecretKey))
        {
            app.UseAuthentication();
        }

        return app;
    }
}
