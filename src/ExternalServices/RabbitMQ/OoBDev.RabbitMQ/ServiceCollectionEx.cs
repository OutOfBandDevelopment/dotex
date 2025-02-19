using OoBDev.MessageQueueing.Services;
using OoBDev.RabbitMQ.MessageQueueing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace OoBDev.RabbitMQ;

/// <summary>
/// Provides extension methods for configuring Rabbit MQ services in the <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionEx
{
    /// <summary>
    /// Tries to add Rabbit MQ services including blob and queue services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <returns>The modified <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection TryAddRabbitMQServices(this IServiceCollection services) =>
        services
            .TryAddRabbitMQQueueServices()
        ;

    /// <summary>
    /// Tries to add Rabbit MQ queue services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <returns>The modified <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection TryAddRabbitMQQueueServices(this IServiceCollection services)
    {
        services.AddTransient<IMessageSenderProvider, RabbitMQQueueMessageProvider>();
        services.TryAddKeyedTransient<IMessageSenderProvider, RabbitMQQueueMessageProvider>(RabbitMQGlobals.MessageProviderKey);

        services.AddTransient<IMessageReceiverProvider, RabbitMQQueueMessageProvider>();
        services.TryAddKeyedTransient<IMessageReceiverProvider, RabbitMQQueueMessageProvider>(RabbitMQGlobals.MessageProviderKey);

        services.TryAddTransient<IQueueClientFactory, QueueClientFactory>();

        return services;
    }
}
