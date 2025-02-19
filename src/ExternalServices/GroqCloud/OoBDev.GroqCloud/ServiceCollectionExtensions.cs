using OoBDev.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System;

namespace OoBDev.GroqCloud;

/// <summary>
/// Provides extension methods for configuring services related to groqCloud.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Configures services for groqCloud.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <param name="configuration">The configuration containing the groqCloud API client options.</param>
    /// <param name="groqCloudApiClientOptionSection">The name of the configuration section containing the groqCloud API client options.</param>
    /// <returns>The modified <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection TryAddGroqCloudServices(
        this IServiceCollection services,
        IConfiguration configuration,
#if DEBUG
        string groqCloudApiClientOptionSection
#else
        string groqCloudApiClientOptionSection = nameof(GroqCloudApiClientOptions)
#endif
        )
    {
        var apiKey =
            configuration.GetSection(groqCloudApiClientOptionSection)?[nameof(GroqCloudApiClientOptions.ApiKey)] ??
            Environment.GetEnvironmentVariable("API_Key_Groq", EnvironmentVariableTarget.User);
        if (apiKey == null)
        {
            return services;
        }
        services.AddHealthChecks().AddCheck<GroqCloudHealthCheck>("groqCloud");

        services.TryAddTransient<IGroqCloudApiClientFactory, GroqCloudApiClientFactory>();
        services.TryAddTransient<IGroqCloudModelMapper, GroqCloudModelMapper>();
        services.TryAddTransient(sp => ActivatorUtilities.CreateInstance<GroqCloudMessageCompletion>(sp));
        services.TryAddTransient(sp => sp.GetRequiredService<IGroqCloudApiClientFactory>().Build());

        services.TryAddTransient<IMessageCompletion, GroqCloudMessageCompletion>();

        services.TryAddKeyedTransient<IMessageCompletion, GroqCloudMessageCompletion>("GroqCloud");

        services.Configure<GroqCloudApiClientOptions>(options => configuration.Bind(groqCloudApiClientOptionSection, options));

        return services;
    }
}
