using Api.Helpers;
using Api.Models;
using Api.Services;
using Data.Models;
using Google.Apis.Auth;
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
        router.MapPost("/api/auth/google", Google);
    }

    private static async Task<IResult> Login(LoginInput input, UserManager<User> userManager, TokenService tokenService, HttpResponse res)
    {
        var user = await userManager.FindByEmailAsync(input.Email);
        // If password hash == null, that means user signed in with google, etc.
        if (user == null || user.PasswordHash == null) return Results.Unauthorized();

        var signed = await userManager.CheckPasswordAsync(user, input.Password);

        if (signed == false) return Results.Unauthorized();

        var token = tokenService.GenerateAccessToken(user);
        res.Cookies.Append("fresh", tokenService.GenerateRefreshToken(user), new() { HttpOnly = true });
        return Results.Ok(token);
    }

    private static async Task<IResult> Register(RegisterInput input, UserManager<User> userManager)
    {
        if (input.Password != input.ConfirmPassword) return Results.BadRequest("Confirm password doesn't match password");

        var userExist = await userManager.FindByEmailAsync(input.Email);
        if (userExist != null) return Results.Conflict("User already exists");

        User user = new()
        {
            UserName = input.Email,
            Email = input.Email,
        };

        var res = await userManager.CreateAsync(user, input.Password);

        if (!res.Succeeded) return Results.Problem(
            statusCode: StatusCodes.Status500InternalServerError,
            detail: "User creation failed! Please check user details and try again."
        );

        return Results.Ok();
    }

    private static async Task<IResult> Google(GoogleInput input, UserManager<User> userManager, TokenService tokenService, HttpResponse response)
    {
        try
        {
            var validPayload = await GoogleJsonWebSignature.ValidateAsync(input.IdToken);

            var user = await userManager.FindByEmailAsync(validPayload.Email);
            if (user == null)
            {
                user = new()
                {
                    Email = validPayload.Email,
                    UserName = validPayload.Email,
                    EmailConfirmed = validPayload.EmailVerified,
                };

                var res = await userManager.CreateAsync(user);

                if (!res.Succeeded) return Results.Problem();
            }

            var token = tokenService.GenerateAccessToken(user);
            response.Cookies.Append("fresh", tokenService.GenerateRefreshToken(user), new() { HttpOnly = true });
            return Results.Ok(token);
        }
        catch
        {
            return Results.BadRequest();
        }
    }
}