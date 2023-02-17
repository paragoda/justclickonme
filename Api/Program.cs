using Api.Db.Context;
using Api.Db.Models;
using Microsoft.EntityFrameworkCore;

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

app.Run();