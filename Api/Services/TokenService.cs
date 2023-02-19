using Api.Helpers;
using Data.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Api.Services;

public class TokenService
{
    private readonly Secrets secrets;

    public TokenService(IOptions<Secrets> secrets)
    {
        this.secrets = secrets.Value;
    }

    public string GenerateAccessToken(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email)
        };

        var expiration = DateTime.UtcNow.AddMinutes(30);

        return GenerateToken(claims, expiration);
    }

    public string GenerateRefreshToken(User user)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
        };

        var expiration = DateTime.UtcNow.AddDays(30);

        return GenerateToken(claims, expiration);
    }

    private string GenerateToken(List<Claim> claims, DateTime expiration)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secrets.JwtKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var jwt = new JwtSecurityToken(
          issuer: secrets.JwtIssuer,
          expires: expiration,
          claims: claims,
          signingCredentials: credentials
        );

        var handler = new JwtSecurityTokenHandler();
        return handler.WriteToken(jwt);
    }
}