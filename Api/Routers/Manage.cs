using Api.Models;
using Data.Context;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Api.Routers;

public static class Manage
{
    public static void MapLinks(this IEndpointRouteBuilder router)
    {
        router.MapGet("/api/links", All).RequireAuthorization();
        router.MapGet("/api/links/{slug:required}", One).RequireAuthorization();
        router.MapPost("/api/links", Create).RequireAuthorization();

        router.MapGet("/api/links/{slug:required}", Delete).RequireAuthorization();
        router.MapPost("/api/links/{slug:required}", Update).RequireAuthorization();
    }

    private static async Task<IResult> All(ClaimsPrincipal user, JustClickOnMeDbContext db)
    {
        try
        {
            var uid = user.FindFirstValue(ClaimTypes.NameIdentifier);
            if (uid == null) return Results.BadRequest();

            var links = await db.Links.Where(l => l.UserId == uid).Select(l => new LinkOutput(l.Slug, l.Destination)).ToListAsync();

            return Results.Ok(links);
        }
        catch
        {
            return Results.Problem();
        }
    }

    // MAYBE not nessasary
    public static async Task<IResult> One(string slug)
    {
        return Results.Ok();
    }

    private static async Task<IResult> Create(CreateLinkInput input, ClaimsPrincipal user, JustClickOnMeDbContext db)
    {
        var uid = user.FindFirstValue(ClaimTypes.NameIdentifier);
        if (uid == null) return Results.Unauthorized();             //why don't we use [Authorize] filter ???

        try
        {
            // TODO: check subcription

            //var path = input.Slug.Trim('/');

            //var scope = path[..path.LastIndexOf('/')];
            //var isOwned = await db.Links.AnyAsync(l => l.Slug.StartsWith(path));

            //if (!isOwned) return Results.BadRequest();

            //var res = await db.Links.AddAsync(new Data.Models.Link(input.Slug, input.Destination, uid));
            //await db.SaveChangesAsync();

            return Results.Ok();
        }
        catch
        {
            return Results.Problem();
        }
    }

    private static async Task<IResult> Delete(string slug, JustClickOnMeDbContext db)
    {
        try
        {
            var entity = await db.Links.FirstAsync(x => x.Slug == slug);
            if (entity != null) db.Links.Remove(entity);
            else return Results.BadRequest();
            await db.SaveChangesAsync();

            return Results.Ok();
        }
        catch
        {
            return Results.Problem();
        }
    }

    private static async Task<IResult> Update(string slug, string slug_new, /*string destination,*/ JustClickOnMeDbContext db)
    {
        try
        {
            var entity = await db.Links.FirstAsync(x => x.Slug == slug);
            if (entity != null)
            {
                entity.Slug = slug_new;
                //entity.Destination = destination;
                db.Entry(entity).State = EntityState.Modified;
            }
            else return Results.BadRequest();

            await db.SaveChangesAsync();

            return Results.Ok();
        }
        catch
        {
            return Results.Problem();
        }
    }

    //What does it mean "Upsert" ??? (Update?/Insert?)
    private static async Task<IResult> Upsert()
    {
        try
        {          
            return Results.Ok();
        }
        catch
        {
            return Results.Problem();
        }
    }
}