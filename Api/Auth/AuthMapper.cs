using Api.Db.Models;
using Api.Helpers;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Api.Auth;

public static class AuthMapper
{
    public static void MapAuth(this IEndpointRouteBuilder router, Secrets secrets)
    {
        // input
        router.MapPost("/api/auth/login", async (
            LoginInput input,
            UserManager<User> userManager,
            SignInManager<User> signInManager
        ) =>
        {
            var user = await userManager.FindByEmailAsync(input.Email);

            var res = await signInManager.PasswordSignInAsync(user, input.Password, true, false);

            if (!res.Succeeded) return Results.Unauthorized();

            // Get token
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secrets.JwtKey));

            var jwt = new JwtSecurityToken(
              issuer: secrets.JwtIssuer,
              audience: secrets.JwtAudience,
              expires: DateTime.UtcNow.AddMinutes(30),
              claims: claims,
              signingCredentials: new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256)
            );

            return Results.Ok(new JwtSecurityTokenHandler().WriteToken(jwt));
        });

        // Registration
        router.MapPost("/api/auth/register", async (RegisterInput input, UserManager<User> userManager) =>
        {
            if (input.Password != input.ConfirmPassword) return Results.BadRequest("Confirm password doesn't match password");

            var userExist = await userManager.FindByEmailAsync(input.Email);
            if (userExist != null) return Results.Conflict("User already exists");

            User user = new()
            {
                UserName = input.Nickname,
                Email = input.Email,
            };

            var res = await userManager.CreateAsync(user, input.Password);
            if (!res.Succeeded) return Results.Problem(
                statusCode: StatusCodes.Status500InternalServerError,
                detail: "User creation failed! Please check user details and try again."
            );

            return Results.Ok();
        });
    }
}