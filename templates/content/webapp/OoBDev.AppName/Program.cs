using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using OoBDev.Common;
using OoBDev.Common.Extensions;

namespace OoBDev.AppName;

/// <summary>
/// Composition root. Infrastructure comes from OoBDev.Common; only application services are added here.
/// </summary>
public static class Program
{
    /// <summary>
    /// Primary entry point.
    /// </summary>
    /// <param name="args">Command line arguments.</param>
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var services = builder.Services;

        // One call per layer; pass builder records only for overrides.
        services.TryAllCommonExtensions(
            builder.Configuration,
            systemBuilder: new(),
            aspNetBuilder: new()
            {
                RequireApplicationUserId = false,
                RequireAuthenticatedByDefault = false,
            },
            jwtBuilder: new(),
            identityBuilder: new(),
            externalBuilder: new(),
            hostingBuilder: new()
            {
                DisableMailKit = true,
            });

        services.AddControllers();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseAllCommonMiddleware(middlewareBuilder: new());

        // Ordering of auth middleware stays in the application.
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();
        app.Run();
    }
}
