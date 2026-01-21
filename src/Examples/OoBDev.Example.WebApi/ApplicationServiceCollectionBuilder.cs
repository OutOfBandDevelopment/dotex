using OoBDev.MessageQueueing;
using OoBDev.WebApi.Provider;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace OoBDev.WebApi;

/// <summary>
/// Provides extension methods for configuring application services in the <see cref="IServiceCollection"/>.
/// </summary>
public static class ApplicationServiceCollectionBuilder
{
    /// <summary>
    /// Adds application services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> instance.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> instance.</returns>
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.TryAddTransient<IExampleMessageProvider, ExampleMessageProvider>();
        services.TryAddTransient<IMessageQueueHandler, ExampleMessageProvider>();
        return services;
    }
}
