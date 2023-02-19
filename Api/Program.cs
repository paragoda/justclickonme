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
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddEndpointsApiExplorer();

// Swagger
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "JustClickOnMe API",
        Version = "v1",
        Contact = new OpenApiContact
        {
            Name = "Flurium Team",
            Email = "fluriumteam@gmail.com",
            Url = new Uri("https://github.com/flurium"),
        },
    });
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Please add token",
        Name = "Auth token",
        Type = SecuritySchemeType.Http,
        BearerFormat = "JWT",
        Scheme = "Bearer"
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
      {
        new OpenApiSecurityScheme
        {
          Reference = new OpenApiReference
          {
              Type= ReferenceType.SecurityScheme,
              Id = "Bearer"
          },
        }, Array.Empty<string>()
      }
    });
    options.SupportNonNullableReferenceTypes();
});

Secrets secrets = new();
if (Env("ASPNETCORE_ENVIRONMENT") == "Production")
{
    secrets.DbConnectionString = Env("DB_CONNECTION_STRING");
    secrets.JwtIssuer = Env("JWT_ISSUER");
    secrets.JwtAudience = Env("JWT_AUDIENCE");
    secrets.JwtKey = Env("JWT_KEY");
}
else
{
    secrets.DbConnectionString = builder.Configuration.GetConnectionString("CockroachDb");
    secrets.JwtIssuer = builder.Configuration["Jwt:Issuer"];
    secrets.JwtAudience = builder.Configuration["Jwt:Audience"];
    secrets.JwtKey = builder.Configuration["Jwt:Key"];
}

// DI for Secrets
builder.Services.Configure<Secrets>(options =>
{
    options.DbConnectionString = secrets.DbConnectionString;
    options.JwtIssuer = secrets.JwtIssuer;
    options.JwtAudience = secrets.JwtAudience;
    options.JwtKey = secrets.JwtKey;
});

// DB
builder.Services.AddDbContext<JustClickOnMeDbContext>(options => options.UseNpgsql(secrets.DbConnectionString));

// AUTH
builder.Services.AddIdentity<User, IdentityRole>().AddEntityFrameworkStores<JustClickOnMeDbContext>().AddDefaultTokenProviders();

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    })
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
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true
        };
    });

builder.Services.AddAuthorization();

var app = builder.Build();

// Always use swagger
app.UseSwagger(options =>
{
    options.RouteTemplate = "api/swagger/{documentname}/swagger.json";
});
app.UseSwaggerUI(options =>
{
    options.RoutePrefix = "api/swagger";
    options.SwaggerEndpoint("/api/swagger/v1/swagger.json", "JustClickOnMe API V1");
});

app.UseHttpsRedirection();

//app.UseCors();
app.UseAuthentication();
app.UseAuthorization();

//app.MapGet("/", () => Results.Redirect("ui.justclickon.me"));

app.MapAuth();

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