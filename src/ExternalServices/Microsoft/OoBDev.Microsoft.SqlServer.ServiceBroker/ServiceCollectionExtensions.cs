using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OoBDev.AsyncApi;
using OoBDev.Microsoft.SqlServer.ServiceBroker.MessageQueueing;
using OoBDev.MessageQueueing.Services;

namespace OoBDev.Microsoft.SqlServer.ServiceBroker;

/// <summary>
/// Provides extension methods for configuring SQL Server Service Broker services in the <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Tries to add Service Broker services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <returns>The modified <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection TryAddServiceBrokerServices(this IServiceCollection services) =>
        services.TryAddServiceBrokerQueueServices();

    /// <summary>
    /// Tries to add the Service Broker queue sender and receiver providers to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <returns>The modified <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection TryAddServiceBrokerQueueServices(this IServiceCollection services)
    {
        services.AddTransient<IMessageSenderProvider, ServiceBrokerQueueMessageProvider>();
        services.TryAddKeyedTransient<IMessageSenderProvider, ServiceBrokerQueueMessageProvider>(ServiceBrokerGlobals.MessageProviderKey);

        services.AddTransient<IMessageReceiverProvider, ServiceBrokerQueueMessageProvider>();
        services.TryAddKeyedTransient<IMessageReceiverProvider, ServiceBrokerQueueMessageProvider>(ServiceBrokerGlobals.MessageProviderKey);

        services.TryAddTransient<IBrokerConnectionFactory, BrokerConnectionFactory>();

        services.TryAddEnumerable(ServiceDescriptor.Singleton<IAsyncApiContributor, ServiceBrokerAsyncApiContributor>());

        return services;
    }
}
