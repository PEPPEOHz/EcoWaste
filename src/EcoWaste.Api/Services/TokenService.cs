using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using EcoWaste.Api.Models;
using Microsoft.IdentityModel.Tokens;

namespace EcoWaste.Api.Services;

public class JwtSettings
{
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public int ExpirationMinutes { get; set; } = 60;
}

public interface ITokenService
{
    (string token, int expiresInSeconds) GenerateToken(AppUser user);
}

public class TokenService : ITokenService
{
    private readonly JwtSettings _settings;

    public TokenService(JwtSettings settings) => _settings = settings;

    public (string token, int expiresInSeconds) GenerateToken(AppUser user)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var expires = DateTime.UtcNow.AddMinutes(_settings.ExpirationMinutes);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: creds);

        return (new JwtSecurityTokenHandler().WriteToken(token), _settings.ExpirationMinutes * 60);
    }
}
