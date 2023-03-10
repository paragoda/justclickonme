using Microsoft.OpenApi.Models;

namespace Api.Configuration;

public static class Swagger
{
    public static void ConfigureSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
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
    }

    public static void UseJustClickOnMeSwagger<TBuilder>(this TBuilder app) where TBuilder : IEndpointRouteBuilder, IApplicationBuilder
    {
        app.UseSwagger(options =>
        {
            options.RouteTemplate = "api/swagger/{documentname}/swagger.json";
        });
        app.UseSwaggerUI(options =>
        {
            string swaggerVersion = "4.15.5";
            string pkgUrl = "https://unpkg.com/swagger-ui-dist@";
            options.HeadContent =
                $"<link rel=\"stylesheet\" type=\"text/css\" " +
                $"href=\"{pkgUrl}{swaggerVersion}/swagger-ui.css\" />";
            options.InjectStylesheet($"{pkgUrl}{swaggerVersion}/swagger-ui.css", "text/css");
            options.InjectJavascript($"{pkgUrl}{swaggerVersion}/swagger-ui-standalone-preset.js", "text/javascript");
            options.InjectJavascript($"{pkgUrl}{swaggerVersion}/swagger-ui-bundle.js", "text/javascript");

            options.SwaggerEndpoint("/api/swagger/v1/swagger.json", "JustClickOnMe API V1");
            options.RoutePrefix = "api/swagger";
        });

        app.MapSwagger();
    }
}