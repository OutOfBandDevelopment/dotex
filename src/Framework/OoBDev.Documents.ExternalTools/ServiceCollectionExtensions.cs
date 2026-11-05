using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using OoBDev.Documents.Conversion;
using OoBDev.System.IO;

namespace OoBDev.Documents.ExternalTools;

/// <summary>Registers external tool conversions.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>Adds one <see cref="IDocumentConversionHandler"/> per tool.</summary>
    /// <param name="services">The service collection.</param>
    /// <param name="tools">The registered tools.</param>
    /// <returns>The same collection.</returns>
    public static IServiceCollection AddExternalToolConversions(this IServiceCollection services, IEnumerable<ExternalToolOptions> tools)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(tools);
        foreach (var tool in tools)
        {
            var copy = ExternalToolOptions.Clone(tool);
            services.AddTransient<IDocumentConversionHandler>(provider =>
                new ExternalToolConversionHandler(copy, provider.GetRequiredService<ITempFileFactory>()));
        }

        return services;
    }
}
