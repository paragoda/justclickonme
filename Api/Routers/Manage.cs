using Api.Models;
using Data.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Win32;
using System.Security.Claims;

namespace Api.Routers;

public static class Manage
{
    public static void MapManage(this IEndpointRouteBuilder router)
    {
        router.MapPost("/api/manage", Create);
    }

    [Authorize]
    private static async Task<IResult> Create(CreateLinkInput input, ClaimsPrincipal user, JustClickOnMeDbContext db)
    {
        var uid = user.FindFirstValue(claimType: ClaimTypes.NameIdentifier);

        if (uid == null) return Results.Unauthorized();

        try
        {
            var res = await db.Links.AddAsync(new Data.Models.Link(input.Slug, input.Destination, uid));
            await db.SaveChangesAsync();

            return Results.Ok();
        }
        catch
        {
            return Results.Problem();
        }
    }
}