#pragma warning disable MA0048 // related small types share a file
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OoBDev.System;
using System;
using System.IO;

namespace OoBDev.AspNetCore.Cgi;

/// <summary>
/// Registration for the CGI middleware.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// The default configuration section.
    /// </summary>
    public const string DefaultSection = "OoBDev:Cgi";

    /// <summary>
    /// Registers the CGI services and binds <see cref="CgiOptions"/>. Options are validated at startup.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration to bind from.</param>
    /// <param name="configurationSectionName">The configuration section name.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection TryAddCgiServices(this IServiceCollection services,
        IConfiguration configuration,
#if DEBUG
        string configurationSectionName
#else
        string configurationSectionName = DefaultSection
#endif
        )
    {
        services.AddValidatedOptions<CgiOptions>(configuration, configurationSectionName);
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IValidateOptions<CgiOptions>, CgiOptionsValidator>());
        services.TryAddSingleton<ICgiPathResolver, CgiPathResolver>();
        services.TryAddSingleton<ICgiProcessRunner, CgiProcessRunner>();
        return services;
    }

    /// <summary>
    /// Adds the CGI middleware. Put authentication and authorization before it.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The application builder for chaining.</returns>
    public static IApplicationBuilder UseCgi(this IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);
        return app.UseMiddleware<CgiMiddleware>();
    }
}

/// <summary>
/// Fails startup when the script root is missing or served as static files.
/// </summary>
/// <param name="environment">The web host environment, used to find the static file root.</param>
public class CgiOptionsValidator(IWebHostEnvironment environment) : IValidateOptions<CgiOptions>
{
    /// <inheritdoc />
    public ValidateOptionsResult Validate(string? name, CgiOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        if (string.IsNullOrWhiteSpace(options.ScriptRoot))
            return ValidateOptionsResult.Fail("ScriptRoot is required.");
        if (!options.Prefix.StartsWith('/'))
            return ValidateOptionsResult.Fail("Prefix must start with a slash.");

        var root = Path.GetFullPath(options.ScriptRoot);
        if (!Directory.Exists(root))
            return ValidateOptionsResult.Fail($"ScriptRoot '{root}' does not exist.");

        if (!string.IsNullOrEmpty(environment.WebRootPath))
        {
            var web = Path.GetFullPath(environment.WebRootPath);
            var comparison = OperatingSystem.IsWindows() ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            var separator = Path.DirectorySeparatorChar;
            if ((root + separator).StartsWith(web + separator, comparison))
                return ValidateOptionsResult.Fail("ScriptRoot must not be under the static file root.");
        }
        return ValidateOptionsResult.Success;
    }
}
