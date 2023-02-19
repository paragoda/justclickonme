using Api.Helpers;
using Api.Models;
using Api.Services;
using Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Api.Routers;

public static class Auth
{
    public static void MapAuth(this IEndpointRouteBuilder router)
    {
        router.MapPost("/api/auth/login", Login);
        router.MapPost("/api/auth/register", Register);
    }

    private static async Task<IResult> Login(LoginInput input, UserManager<User> userManager, TokenService tokenService)
    {
        var user = await userManager.FindByEmailAsync(input.Email);
        var signed = await userManager.CheckPasswordAsync(user, input.Password);

        if (user == null || signed == false) return Results.Unauthorized();

        var token = tokenService.GenerateAccessToken(user);
        return Results.Ok(token);
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