using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OoBDev.MessageQueueing.Services;
using OoBDev.Microsoft.Azure.ServiceBus.MessageQueueing;

namespace OoBDev.Microsoft.Azure.ServiceBus;

/// <summary>
/// Provides extension methods for configuring Azure Service Bus services in the <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionEx
{
    /// <summary>
    /// Tries to add Azure Service Bus services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <returns>The modified <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection TryAddAzureServiceBusServices(this IServiceCollection services)
    {
        // Non-keyed registration (default provider)
        services.TryAddTransient<IMessageSenderProvider, AzureServiceBusMessageProvider>();

        // Keyed registration (for multi-provider scenarios)
        services.AddKeyedTransient<IMessageSenderProvider, AzureServiceBusMessageProvider>(
            AzureServiceBusGlobals.MessageProviderKey
        );

        // Factory registration
        services.TryAddTransient<IServiceBusSenderFactory, ServiceBusSenderFactory>();

        return services;
    }
}
