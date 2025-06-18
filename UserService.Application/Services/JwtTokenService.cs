using System.Security.Claims;
using System.Text;
using UserService.Domain.JWT;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;
using System.IdentityModel.Tokens.Jwt;

namespace UserService.Application.Services;

public class JwtTokenService : IJwtTokenService
{
    private readonly JwtSettings _settings;

    public JwtTokenService(IOptions<JwtSettings> settings)
    {
        if (settings == null)
            throw new ArgumentNullException(nameof(settings));
        _settings = settings.Value;
    }

    public string GenerateToken(int userId, List<string> roles)
    {
        var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, userId.ToString())
            };

        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_settings.ExpiresInMinutes),
            signingCredentials: creds);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}