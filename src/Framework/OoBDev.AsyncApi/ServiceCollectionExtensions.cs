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

    /// <summary>Lists a message type that the application sends, so its channel gets a schema in the document.</summary>
    public static IServiceCollection AddAsyncApiSendMessage<TChannel, TMessage>(this IServiceCollection services)
    {
        services.AddSingleton(new AsyncApiSendMessage(typeof(TChannel), typeof(TMessage)));
        return services;
    }
}
