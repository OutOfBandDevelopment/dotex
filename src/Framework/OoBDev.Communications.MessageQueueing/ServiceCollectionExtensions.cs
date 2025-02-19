using OoBDev.MessageQueueing;
using Microsoft.Extensions.DependencyInjection;

namespace OoBDev.Communications.MessageQueueing;

/// <summary>
/// Extension methods for configuring communication queue services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Tries to add communication queue services to the specified service collection.
    /// </summary>
    /// <param name="services">The service collection to which communication queue services should be added.</param>
    /// <returns>The modified service collection.</returns>
    public static IServiceCollection TryAddCommunicationQueueServices(this IServiceCollection services)
    {
        // Adds a transient registration for the email message handler
        services.AddTransient<IMessageQueueHandler, EmailMessageHandler>();

        return services;
    }
}
