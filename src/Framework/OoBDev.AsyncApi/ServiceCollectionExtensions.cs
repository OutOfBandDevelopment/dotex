using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace OoBDev.AsyncApi;

/// <summary>Registration for the AsyncAPI document builder.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>Adds <see cref="AsyncApiDocumentBuilder"/>. Adapters add their own <see cref="IAsyncApiContributor"/>.</summary>
    public static IServiceCollection TryAddAsyncApiServices(this IServiceCollection services)
    {
        services.TryAddTransient<AsyncApiDocumentBuilder>();
        return services;
    }
}
