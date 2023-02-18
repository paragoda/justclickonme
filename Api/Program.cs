using Api.Db.Context;
using Api.Db.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Builder;
using Api.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Api.Helpers;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options => options.SupportNonNullableReferenceTypes());

Secrets secrets = Env("ASPNETCORE_ENVIRONMENT") == "Development"
    ? new(
        dbConnectionString: Env("DB_CONNECTION_STRING"),
        jwtIssuer: Env("JWT_ISSUER"),
        jwtAudience: Env("JWT_AUDIENCE"),
        jwtKey: Env("JWT_KEY")
    )
    : new(
        dbConnectionString: builder.Configuration.GetConnectionString("DB_CONNECTION_STRING"),
        jwtIssuer: builder.Configuration["Jwt:Issuer"],
        jwtAudience: builder.Configuration["Jwt:Audience"],
        jwtKey: builder.Configuration["Jwt:Key"]
    );

// DB
builder.Services.AddDbContext<JustClickOnMeDbContext>(options => options.UseNpgsql(secrets.DbConnectionString));

// AUTH
builder.Services.AddIdentity<User, IdentityRole>().AddEntityFrameworkStores<JustClickOnMeDbContext>().AddDefaultTokenProviders();
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.SaveToken = true;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidIssuer = secrets.JwtIssuer,
            ValidAudience = secrets.JwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secrets.JwtKey)),
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true
        };
    });

builder.Services.AddAuthorization();

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

//app.MapGet("/", () => Results.Redirect("ui.justclickon.me"));

app.MapAuth(secrets);

//app.MapGet("/{**slug}", async (string slug, JustClickOnMeDbContext db) =>
//{
//    var link = await db.Links.FirstOrDefaultAsync(l => l.Slug == slug);
//    if (link == null) return Results.NotFound();

//    if (link.ExpireTime <= DateTime.Now) return Results.NoContent();

//    if (link.Password != null) return Results.Redirect($"ui.justclickon.me/private/{link.Slug}");

//    return Results.Redirect(link.Destination);
//});

app.Run();

static string Env(string variable) => Environment.GetEnvironmentVariable(variable) ?? "";