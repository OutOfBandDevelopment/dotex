using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.OData;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OoBDev.System;

namespace OoBDev.AspNetCore.OData;

/// <summary>
/// Registration for the OData query surface.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// The default configuration section.
    /// </summary>
    public const string DefaultSection = "OoBDev:OData";

    /// <summary>
    /// Registers the OData query services and binds <see cref="ODataQueryLimits"/>. Options are validated at startup.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration to bind from.</param>
    /// <param name="configurationSectionName">The configuration section name.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection TryAddODataQueryServices(this IServiceCollection services,
        IConfiguration configuration,
#if DEBUG
        string configurationSectionName
#else
        string configurationSectionName = DefaultSection
#endif
        )
    {
        services.AddValidatedOptions<ODataQueryLimits>(configuration, configurationSectionName);
        services.AddControllers().AddOData(odata => odata.Select().Filter().OrderBy().Count().Expand().SetMaxTop(null));
        return services;
    }

    /// <summary>
    /// Adds the form body translation. Put it after <c>UseHttpQueryVerb()</c> and before routing.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The application builder for chaining.</returns>
    public static IApplicationBuilder UseODataQueryBody(this IApplicationBuilder app) => app.UseMiddleware<ODataQueryBodyMiddleware>();
}
