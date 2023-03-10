using Api.Configuration;
using Api.Models;
using Api.Services;
using Data.Context;
using Data.Models;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace Api.Routers;

public static class AuthRouter
{
    public static void MapAuth(this IEndpointRouteBuilder router)
    {
        router.MapPost("/api/auth/login", Login);
        router.MapPost("/api/auth/register", Register);
        router.MapPost("/api/auth/google", Google);
        router.MapGet("/api/auth/refresh", Refresh);

        // now for testing
        router.MapPost("/api/auth/revoke", Revoke);
    }

    private static void SetCookie(HttpResponse response, string key, string value)
    {
        response.Cookies.Append(key, value, new()
        {
            HttpOnly = true,
            SameSite = SameSiteMode.None,
            Secure = true
        });
    }

    private static async Task<IResult> Login(LoginInput input, UserManager<User> userManager, TokenService tokenService, HttpResponse response)
    {
        var user = await userManager.FindByEmailAsync(input.Email);
        // If password hash == null, that means user signed in with google, etc.
        if (user == null || user.PasswordHash == null) return Results.Unauthorized();

        var signed = await userManager.CheckPasswordAsync(user, input.Password);

        if (signed == false) return Results.Unauthorized();

        var token = tokenService.GenerateAccessToken(user);
        SetCookie(response, Constants.RefreshTokenCookie, tokenService.GenerateRefreshToken(user));

        return Results.Ok(new
        {
            accessToken = token
        });
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
            SetCookie(response, Constants.RefreshTokenCookie, tokenService.GenerateRefreshToken(user));
            return Results.Ok(new
            {
                accessToken = token
            });
        }
        catch
        {
            return Results.BadRequest();
        }
    }

    private static async Task<IResult> Refresh(HttpRequest request, TokenService tokenService, UserManager<User> userManager, HttpResponse response)
    {
        try
        {
            var refreshToken = request.Cookies[Constants.RefreshTokenCookie];
            if (refreshToken == null) return Results.BadRequest();

            var tokenValidation = await tokenService.VerifyRefreshToken(refreshToken);
            if (!tokenValidation.IsValid) return Results.BadRequest();

            var uid = tokenValidation.ClaimsIdentity.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (uid == null) return Results.BadRequest();

            var user = await userManager.FindByIdAsync(uid);
            if (user == null) return Results.NotFound();

            var refreshClaim = tokenValidation.ClaimsIdentity.FindFirst(Constants.RefreshTokenVersion);
            if (refreshClaim == null) return Results.BadRequest();

            var refreshTokenVersion = int.Parse(refreshClaim.Value);

            if (user.RefreshTokenVersion != refreshTokenVersion) return Results.Unauthorized();

            var accessToken = tokenService.GenerateAccessToken(user);
            SetCookie(response, Constants.RefreshTokenCookie, tokenService.GenerateRefreshToken(user));
            return Results.Ok(new
            {
                accessToken
            });
        }
        catch
        {
            return Results.BadRequest();
        }
    }

    private static async Task<IResult> Revoke(ClaimsPrincipal claims, JustClickOnMeDbContext db)
    {
        var uid = claims.FindFirstValue(claimType: ClaimTypes.NameIdentifier);
        if (uid == null) return Results.NotFound();

        var user = db.Users.FirstOrDefault(u => u.Id == uid);
        if (user == null) return Results.NotFound();

        user.RefreshTokenVersion += 1;
        await db.SaveChangesAsync();

        return Results.Ok();
    }
}