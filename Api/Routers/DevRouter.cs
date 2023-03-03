using Data.Context;

namespace Api.Routers
{
    public static class DevRouter
    {
        public static void MapDev(this IEndpointRouteBuilder router)
        {
            router.MapGet("/api/dev", GetDevTokens);
        }

        public static async Task<IResult> GetDevTokens(JustClickOnMeDbContext db)
        {
            return Results.Ok();
        }
    }
}