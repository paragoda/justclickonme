using Api.Configuration;
using Api.Helpers;
using Api.Routers;
using Api.Services;
using Data.Context;
using Data.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();

// Configure swagger
builder.Services.ConfigureSwagger();

Secrets secrets = new();
if (Env("ASPNETCORE_ENVIRONMENT") == "Production")
{
    secrets.DbConnectionString = Env("DB_CONNECTION_STRING");
    secrets.JwtIssuer = Env("JWT_ISSUER");
    secrets.JwtAccessSecret = Env("JWT_ACCESS_SECRET");
    secrets.JwtRefreshSecret = Env("JWT_REFRESH_SECRET");

    secrets.GoogleClientId = Env("GOOGLE_CLIENT_ID");
    secrets.GoogleClientSecret = Env("GOOGLE_CLIENT_SECRET");
}
else
{
    secrets.DbConnectionString = builder.Configuration.GetConnectionString("CockroachDb");

    secrets.JwtIssuer = builder.Configuration["Jwt:Issuer"];
    secrets.JwtAccessSecret = builder.Configuration["Jwt:AccessSecret"];
    secrets.JwtRefreshSecret = builder.Configuration["Jwt:RefreshSecret"];

    secrets.GoogleClientId = builder.Configuration["GoogleClient:Id"];
    secrets.GoogleClientSecret = builder.Configuration["GoogleClient:Secret"];
}

// DI for Secrets
builder.Services.Configure<Secrets>(options =>
{
    options.DbConnectionString = secrets.DbConnectionString;

    options.JwtIssuer = secrets.JwtIssuer;
    options.JwtAccessSecret = secrets.JwtAccessSecret;
    options.JwtRefreshSecret = secrets.JwtRefreshSecret;

    options.GoogleClientId = secrets.GoogleClientId;
    options.GoogleClientSecret = secrets.GoogleClientSecret;
});

// Db
builder.Services.AddDbContext<JustClickOnMeDbContext>(options => options.UseNpgsql(secrets.DbConnectionString));

// Auth
builder.Services.AddIdentity<User, IdentityRole>().AddEntityFrameworkStores<JustClickOnMeDbContext>().AddDefaultTokenProviders();
builder.Services.AddJwtAuthentication(secrets);
builder.Services.AddAuthorization();

builder.Services.AddTransient<TokenService>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyHeader()
            .WithMethods("GET", "POST", "DELETE", "PUT");
    });
});

var app = builder.Build();

// Use swagger
app.UseJustClickOnMeSwagger();

app.UseHttpsRedirection();

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

// Map Routers
app.MapAuth();
app.MapLinks();
app.MapRedirector();

app.Run();

static string Env(string variable) => Environment.GetEnvironmentVariable(variable) ?? "";