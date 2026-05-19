using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;

namespace BlogSharp.Api.Security;

public static class AuthUserExtensions
{
    public static long? ObterUsuarioId(this ControllerBase controller)
    {
        var usuarioId = controller.User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? controller.User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        return long.TryParse(usuarioId, out var id) ? id : null;
    }

    public static bool UsuarioEhAdmin(this ControllerBase controller)
    {
        return controller.User.IsInRole("Admin");
    }
}
