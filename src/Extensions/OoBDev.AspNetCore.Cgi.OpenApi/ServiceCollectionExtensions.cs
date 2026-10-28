using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OoBDev.System;
using System;

namespace OoBDev.AspNetCore.Cgi.OpenApi;

/// <summary>
/// Registration for the CGI OpenAPI extension.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// The default configuration section.
    /// </summary>
    public const string DefaultSection = "OoBDev:Cgi:OpenApi";

    /// <summary>
    /// Lists the CGI programs that have a description file in the OpenAPI documents. Register the CGI services (<c>TryAddCgiServices</c>) as well.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration to bind from.</param>
    /// <param name="configurationSectionName">The configuration section name.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection TryAddCgiOpenApi(this IServiceCollection services,
        IConfiguration configuration,
#if DEBUG
        string configurationSectionName
#else
        string configurationSectionName = DefaultSection
#endif
        )
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddValidatedOptions<CgiOpenApiOptions>(configuration, configurationSectionName);
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigureOptions<OpenApiOptions>, ConfigureCgiOpenApiOptions>());
        return services;
    }
}
