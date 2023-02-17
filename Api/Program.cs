using Api.Db.Context;
using Api.Db.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Api.Auth;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// DB
string connectionString;
if (Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development")
{
    connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING") ?? "";
}
else
{
    connectionString = builder.Configuration.GetConnectionString("DB_CONNECTION_STRING");
}
builder.Services.AddDbContext<JustClickOnMeDbContext>(options => options.UseNpgsql(connectionString));

// AUTH
builder.Services.AddIdentityCore<User>().AddEntityFrameworkStores<JustClickOnMeDbContext>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/", () => Results.Redirect("ui.justclickon.me"));

app.MapAuth();

app.MapGet("/{**slug}", async (string slug, JustClickOnMeDbContext db) =>
{
    var link = await db.Links.FirstOrDefaultAsync(l => l.Slug == slug);
    if (link == null) return Results.NotFound();

    if (link.ExpireTime <= DateTime.Now) return Results.NoContent();

    if (link.Password != null) return Results.Redirect($"ui.justclickon.me/private/{link.Slug}");

    return Results.Redirect(link.Destination);
});

app.Run();