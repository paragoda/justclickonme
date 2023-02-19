using Microsoft.AspNetCore.Authorization;
using Microsoft.Win32;

namespace Api.Routers;

public static class Manage
{
    public static void MapManage(this IEndpointRouteBuilder router)
    {
        router.MapGet("/api/manage/create", Create);
    }

    [Authorize]
    private static async Task<string> Create()
    {
        return "yeah";
    }
}