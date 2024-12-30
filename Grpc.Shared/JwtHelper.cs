using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Grpc.Shared;

public static class JwtHelper
{
    public static readonly SymmetricSecurityKey SecurityKey = new(Encoding.UTF8.GetBytes("SuperDuperSecretSecurityKeyForTesting"));

    public static string GenerateJwtToken(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, name),
            new Claim(ClaimTypes.Role, "Admin")
        };

        var credentials = new SigningCredentials(
            SecurityKey,
            SecurityAlgorithms.HmacSha256
        );

        var token = new JwtSecurityToken(
            "issuer",
            "audience",
            claims,
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}