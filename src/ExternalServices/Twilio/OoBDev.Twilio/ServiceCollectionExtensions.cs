using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OoBDev.Communications.Abstractions;

namespace OoBDev.Twilio;

/// <summary>Registers the Twilio provider.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>Adds the <c>twilio</c> sms provider when the section holds an <see cref="TwilioOptions.AccountSid"/>.</summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <param name="configurationSection">The section name.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection TryAddTwilioProvider(this IServiceCollection services, IConfiguration configuration,
#if DEBUG
        string configurationSection
#else
        string configurationSection = TwilioGlobals.ConfigurationSection
#endif
        )
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        var section = configuration.GetSection(configurationSection);
        if (string.IsNullOrWhiteSpace(section[nameof(TwilioOptions.AccountSid)]))
        {
            return services;
        }

        services.TryAddSingleton(TimeProvider.System);
        services.AddOptions<TwilioOptions>().Bind(section).ValidateDataAnnotations();
        services.AddHttpClient<TwilioChannelProvider>();
        services.TryAddEnumerable(ServiceDescriptor.Transient<IChannelProvider, TwilioChannelProvider>());
        return services;
    }
}
