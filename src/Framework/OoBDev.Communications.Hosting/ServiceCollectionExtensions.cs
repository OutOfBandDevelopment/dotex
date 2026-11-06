using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace OoBDev.Communications.Hosting;

/// <summary>Extension methods that host the communications background work.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>Adds <see cref="ReceivePollingHost"/>, which polls the channels that set a <c>PollInterval</c>.</summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The modified <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection TryAddCommunicationsHosting(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.TryAddSendReceive();
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IHostedService, ReceivePollingHost>());
        return services;
    }
}
