using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OoBDev.Communications.Abstractions;

namespace OoBDev.SendGrid;

/// <summary>Registers the SendGrid provider.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>Adds the <c>sendgrid</c> email provider when the section holds an <see cref="SendGridOptions.ApiKey"/>.</summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <param name="configurationSection">The section name.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection TryAddSendGridProvider(this IServiceCollection services, IConfiguration configuration,
#if DEBUG
        string configurationSection
#else
        string configurationSection = SendGridGlobals.ConfigurationSection
#endif
        )
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var section = configuration.GetSection(configurationSection);
        if (string.IsNullOrWhiteSpace(section[nameof(SendGridOptions.ApiKey)]))
        {
            return services;
        }

        services.TryAddSingleton(TimeProvider.System);
        services.AddOptions<SendGridOptions>().Bind(section).ValidateDataAnnotations();
        services.AddHttpClient<SendGridChannelProvider>();
        services.TryAddEnumerable(ServiceDescriptor.Transient<IChannelProvider, SendGridChannelProvider>());
        return services;
    }
}
