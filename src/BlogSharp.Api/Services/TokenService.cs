using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BlogSharp.Api.Models;
using Microsoft.IdentityModel.Tokens;

namespace BlogSharp.Api.Services;

public class TokenService(IConfiguration configuration) : ITokenService
{
    public string GerarToken(Usuario usuario)
    {
        var secretKey = configuration["Jwt:SecretKey"];

        if (string.IsNullOrWhiteSpace(secretKey))
        {
            throw new InvalidOperationException("Jwt:SecretKey nao configurada.");
        }

        var issuer = configuration["Jwt:Issuer"];
        var audience = configuration["Jwt:Audience"];
        var expirationMinutes = configuration.GetValue("Jwt:ExpirationMinutes", 60);
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, usuario.Email),
            new(ClaimTypes.Name, usuario.Nome),
            new(ClaimTypes.Role, usuario.Tipo)
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
