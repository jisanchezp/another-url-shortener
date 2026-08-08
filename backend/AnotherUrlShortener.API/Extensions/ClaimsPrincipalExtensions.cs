
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace AnotherUrlShortener.API.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid? GetUserId(this ClaimsPrincipal user)
    {
        var value = user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

        return Guid.TryParse(value, out var id) ? id : null;
    }
}