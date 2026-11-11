using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OoBDev.AsyncApi;
using OoBDev.MessageQueueing.Services;
using OoBDev.PostgreSql.Pgmq.MessageQueueing;

namespace OoBDev.PostgreSql.Pgmq;

/// <summary>
/// Registers the pgmq queue adapter.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the pgmq sender and receiver providers, also under the key <see cref="PgmqGlobals.MessageProviderKey"/>.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection TryAddPgmqQueueServices(this IServiceCollection services)
    {
        services.AddTransient<IMessageSenderProvider, PgmqQueueMessageProvider>();
        services.TryAddKeyedTransient<IMessageSenderProvider, PgmqQueueMessageProvider>(PgmqGlobals.MessageProviderKey);

        services.AddTransient<IMessageReceiverProvider, PgmqQueueMessageProvider>();
        services.TryAddKeyedTransient<IMessageReceiverProvider, PgmqQueueMessageProvider>(PgmqGlobals.MessageProviderKey);

        services.TryAddTransient<IPgmqClientFactory, PgmqClientFactory>();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IAsyncApiContributor, PgmqAsyncApiContributor>());
        return services;
    }
}
