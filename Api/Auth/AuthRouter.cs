using Api.Db.Models;
using Api.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Api.Auth;

public static class AuthRouter
{
    public static void MapAuth(this IEndpointRouteBuilder router)
    {
        router.MapPost("/api/auth/login", Login);
        router.MapPost("/api/auth/register", Register);
    }

    private static async Task<IResult> Login(LoginInput input, UserManager<User> userManager, IOptions<Secrets> secrets)
    {
        var user = await userManager.FindByEmailAsync(input.Email);
        var signed = await userManager.CheckPasswordAsync(user, input.Password);

        if (user == null || signed == false) return Results.Unauthorized();

        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Email, user.Email)
        };

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secrets.Value.JwtKey));

        var jwt = new JwtSecurityToken(
          issuer: secrets.Value.JwtIssuer,
          audience: secrets.Value.JwtAudience,
          expires: DateTime.UtcNow.AddMinutes(30),
          claims: claims,
          signingCredentials: new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256)
        );

        return Results.Ok(new JwtSecurityTokenHandler().WriteToken(jwt));
    }

    private static async Task<IResult> Register(RegisterInput input, UserManager<User> userManager)
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
    }
}