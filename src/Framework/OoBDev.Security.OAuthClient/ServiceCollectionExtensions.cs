using System;
using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace OoBDev.Security.OAuthClient;

/// <summary>Registers the OAuth client.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>Registers <see cref="IOAuthClient"/>, <see cref="ITokenCache"/>, a file secret store and <see cref="BearerTokenHandler"/>, bound to <c>OoBDev:OAuthClient</c>. Replace <see cref="ISecretStore"/> before this call to use another store.</summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration holding the section.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection AddOAuthClient(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        services.AddOptions<OAuthClientOptions>().Bind(configuration.GetSection(OAuthClientOptions.SectionName)).ValidateDataAnnotations().ValidateOnStart();
        services.TryAddSingleton(TimeProvider.System);
        services.TryAddSingleton<ISecretStore, FileSecretStore>();
        services.AddHttpClient<PublicOAuthClient>();
        services.TryAddSingleton<IOAuthClient>(sp => sp.GetRequiredService<PublicOAuthClient>());
        services.TryAddSingleton<ITokenCache, TokenCache>();
        services.TryAddTransient<BearerTokenHandler>();
        return services;
    }

    /// <summary>Adds <see cref="BearerTokenHandler"/> to a named or typed HTTP client so its requests carry the signed-in user's token.</summary>
    /// <param name="builder">The HTTP client builder.</param>
    /// <returns>The builder.</returns>
    public static IHttpClientBuilder AddBearerToken(this IHttpClientBuilder builder) => builder.AddHttpMessageHandler<BearerTokenHandler>();
}
