using BlogSharp.Api.Models;

namespace BlogSharp.Api.Services;

public interface ITokenService
{
    string GerarToken(Usuario usuario);
}
