using Api.Auth.Models;
using Api.Db.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Api.Auth;

internal record RegisterInput(string Nickname, string Email, string Password, string ConfirmPassword);
internal record LoginInput(string Email, string Password);

public static class AuthMapper
{
    public static void MapAuth(this IEndpointRouteBuilder router)
    {
        router.MapPost("/api/auth/login", async (LoginInput input, UserManager<User> userManager, SignInManager<User> signInManager) =>
        {
            var user = await userManager.FindByEmailAsync(input.Email);
            var res = await signInManager.PasswordSignInAsync(user, input.Password, true, false);

            if (!res.Succeeded) return Results.Unauthorized();

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(user.Email));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var jwt = new JwtSecurityToken(
              issuer: "ABCXYZ",
              claims: new List<Claim> { },
              expires: DateTime.UtcNow.AddMinutes(10),
              signingCredentials: credentials
            );

            return Results.Ok(new JwtSecurityTokenHandler().WriteToken(jwt));
        });

        router.MapPost("/api/auth/register", (RegisterInput input) =>
        {
            if (input.Password != input.ConfirmPassword) return Results.BadRequest();

            return Results.Ok();
        });
    }
}