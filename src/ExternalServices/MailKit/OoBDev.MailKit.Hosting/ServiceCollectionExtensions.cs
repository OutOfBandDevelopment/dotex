using Microsoft.Extensions.DependencyInjection;

namespace OoBDev.MailKit.Hosting;

/// <summary>
/// Provides extension methods for configuring IoC (Inversion of Control) services
/// to support all Message Queueing within this library.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Add IOC configurations to support all Mailkit Hosting within this library.
    /// </summary>
    /// <param name="services"></param>
    /// <returns></returns>
    public static IServiceCollection TryAddMailKitHosting(this IServiceCollection services)
    {
        //Note: this is the service host to enable the inbound message handlers
        services.AddHostedService<EmailMessageReceiverHost>();
        return services;
    }
}
