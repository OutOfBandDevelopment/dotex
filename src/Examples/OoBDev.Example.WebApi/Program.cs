using OoBDev.AspNetCore.JwtAuthentication.SwaggerGen;
using OoBDev.Common;
using OoBDev.Common.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;

namespace OoBDev.WebApi;

/// <summary>
/// primary entry point
/// </summary>
public static class Program
{
    /// <summary>
    /// primary entry point
    /// </summary>
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        var services = builder.Services;

        // Add example application services 
        services.AddApplicationServices();

        var identityProvider = Enum.TryParse<IdentityProviders>(
            Environment.GetEnvironmentVariable("IDENTITY_PROVIDER"), ignoreCase: true, out var ip) ? ip :
            IdentityProviders.None;
        var authProvider = identityProvider != IdentityProviders.None ? $"{identityProvider}:" : "";
        var skipHosting = bool.TryParse(Environment.GetEnvironmentVariable("SWAGGER_ONLY"), out var ret) && ret;

        // Add internal services
        services.TryAllCommonExtensions(
            builder.Configuration,
            systemBuilder: new()
            {
            },
            aspNetBuilder: new()
            {
                RequireApplicationUserId = false,
                RequireAuthenticatedByDefault = identityProvider != IdentityProviders.None,
            },
            jwtBuilder: new()
            {
                JwtBearerConfigurationSection = authProvider + nameof(JwtBearerOptions),
                OAuth2SwaggerConfigurationSection = authProvider + nameof(OAuth2SwaggerOptions),
            },
            identityBuilder: new()
            {
                IdentityProvider = identityProvider,
            },
            externalBuilder: new()
            {
            },
            hostingBuilder: new()
            {
                //TODO: add a run profile that disable hosting
                DisableMailKit = true, //TODO: this is not supported at this time so just skip it ...skipHosting,
                //DisableMessageQueueing = true,  // skipHosting,
            });

        // Add services to the container.
        services.AddLogging(builder => builder
            .AddConsole()
#if DEBUG
            .SetMinimumLevel(LogLevel.Debug)
#endif
        );

        builder.Services.AddControllers();
        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        app.UseAllCommonMiddleware(
            middlewareBuilder: new()
            {
            });

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}
