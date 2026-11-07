using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OoBDev.Communications.Abstractions;

namespace OoBDev.Amazon.Ses;

/// <summary>Registration of the AWS SES adapter.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>Adds the SES channel provider and the SNS delivery webhook processor when the <c>SesOptions</c> section exists.</summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <param name="sectionName">The options section name.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection TryAddAmazonSesServices(this IServiceCollection services, IConfiguration configuration, string sectionName = nameof(SesOptions))
    {
        ArgumentNullException.ThrowIfNull(configuration);
        var section = configuration.GetSection(sectionName);
        if (!section.Exists())
        {
            return services;
        }

        services.AddOptions<SesOptions>().Bind(section);
        services.TryAddSingleton(TimeProvider.System);
        services.TryAddTransient<ISesClientFactory, SesClientFactory>();
        services.TryAddEnumerable(ServiceDescriptor.Transient<IChannelProvider, SesChannelProvider>());
        services.TryAddEnumerable(ServiceDescriptor.Transient<IWebhookProcessor, SnsDeliveryWebhookProcessor>());
        return services;
    }
}
