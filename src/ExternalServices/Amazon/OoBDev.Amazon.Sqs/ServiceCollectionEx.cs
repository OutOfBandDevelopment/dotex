using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OoBDev.Amazon.Sqs.MessageQueueing;
using OoBDev.MessageQueueing.Services;

namespace OoBDev.Amazon.Sqs;

/// <summary>
/// Provides extension methods for configuring AWS SQS services in the <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionEx
{
    /// <summary>
    /// Tries to add AWS SQS services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <returns>The modified <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection TryAddAmazonSqsServices(this IServiceCollection services)
    {
        // Non-keyed registration (default provider)
        services.TryAddTransient<IMessageSenderProvider, AmazonSqsMessageProvider>();

        // Keyed registration (for multi-provider scenarios)
        services.AddKeyedTransient<IMessageSenderProvider, AmazonSqsMessageProvider>(
            AwsSqsGlobals.MessageProviderKey
        );

        // Factory registration
        services.TryAddTransient<ISqsClientFactory, SqsClientFactory>();

        return services;
    }
}
