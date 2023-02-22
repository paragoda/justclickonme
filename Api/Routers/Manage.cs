using Microsoft.AspNetCore.Authorization;
using Microsoft.Win32;
using System.Security.Claims;

namespace Api.Routers;

public static class Manage
{
    public static void MapManage(this IEndpointRouteBuilder router)
    {
        router.MapGet("/api/manage/create", Create);
    }

    [Authorize]
    private static async Task<IResult> Create(ClaimsPrincipal user)
    {
        var id = user.FindFirstValue(claimType: ClaimTypes.NameIdentifier);
        return Results.Ok(id);
    }
}