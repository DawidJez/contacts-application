using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Contacts.Auth;

public sealed class JwtTokenService
{
    private readonly IConfiguration _config;

    public JwtTokenService(IConfiguration config) => _config = config;

    public string CreateToken(int userId, string email)
    {
        var jwt = _config.GetSection("Jwt");
        var key = jwt["Key"]!;
        var issuer = jwt["Issuer"] ?? "ContactsApi";
        var audience = jwt["Audience"] ?? "ContactsSpa";
        var expiresMinutes = int.TryParse(jwt["ExpiresMinutes"], out var m) ? m : 60;

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Email, email),
        };

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var creds = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expiresMinutes),
            signingCredentials: creds
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}