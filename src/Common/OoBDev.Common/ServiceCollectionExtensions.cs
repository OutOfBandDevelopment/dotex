using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OoBDev.Communications;
using OoBDev.Communications.MessageQueueing;
using OoBDev.Data.Common;
using OoBDev.Documents;
using OoBDev.Identity;
using OoBDev.MessageQueueing;
using OoBDev.Search;
using OoBDev.SemanticKernel;
using OoBDev.System;

namespace OoBDev.Common;

/// <summary>
/// Provides extension methods for configuring common services in the <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Tries to add common services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> instance.</param>
    /// <param name="configuration">The configuration containing settings for common services.</param>
    /// <param name="builder">Optional builder for configuring system extensions. Default is <c>null</c>.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> instance.</returns>
    public static IServiceCollection TryCommonExtensions(
        this IServiceCollection services,
        IConfiguration configuration,
#if DEBUG
        SystemExtensionBuilder? builder
#else
        SystemExtensionBuilder? builder = default
#endif
    )
    {
        // Add system extensions
        services.TryAddSystemExtensions(configuration, builder);

        // Add common services
        services.TryAddMessageQueueingServices();
        services.TryAddCommunicationQueueServices();
        services.TryAddCommunicationsServices();
        services.TryAddIdentityServices(configuration);
        services.TryAddSearchServices();
        services.TryAddDocumentServices();
        services.TryAddDataCommonServices();
        services.TryAddSemanticKernelPlugins();

        return services;
    }
}
