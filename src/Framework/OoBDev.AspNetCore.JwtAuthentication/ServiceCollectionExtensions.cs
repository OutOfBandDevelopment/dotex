using OoBDev.AspNetCore.JwtAuthentication.OpenApi;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using OoBDev.AspNetCore.Mvc.OpenApi;

namespace OoBDev.AspNetCore.JwtAuthentication;

/// <summary>
/// Extension methods for configuring JWT Bearer authentication and OpenAPI services in <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Tries to add JWT Bearer authentication and OpenAPI services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <param name="configuration">The configuration.</param>
    /// <param name="builder">The default authentication scheme.</param>
    /// <returns>The modified <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection TryAddJwtBearerServices(
        this IServiceCollection services,
        IConfiguration configuration,
#if DEBUG
        JwtExtensionBuilder? builder
#else
        JwtExtensionBuilder? builder = default
#endif
    )
    {
        builder ??= new();
        services.TryAddJwtBearerAuthentication(configuration, builder.DefaultSchema, builder.JwtBearerConfigurationSection);
        services.TryAddJwtBearerOpenApi(configuration, builder.OAuth2OpenApiConfigurationSection);
        return services;
    }

    /// <summary>
    /// Tries to add JWT Bearer authentication services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <param name="configuration">The configuration.</param>
    /// <param name="defaultScheme">The default authentication scheme.</param>
    /// <param name="configurationSection">The configuration section for JwtBearer options.</param>
    /// <returns>The modified <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection TryAddJwtBearerAuthentication(
         this IServiceCollection services,
         IConfiguration configuration,
#if DEBUG
         string defaultScheme,
         string configurationSection
#else
         string defaultScheme = JwtBearerDefaults.AuthenticationScheme,
         string configurationSection = nameof(JwtBearerOptions)
#endif
    )
    {
        services.Configure<JwtBearerOptions>(options => configuration.Bind(configurationSection, options));
        services
            .AddAuthentication(defaultScheme)
            .AddJwtBearer(options => configuration.Bind(configurationSection, options));

        return services;
    }

    /// <summary>
    /// Tries to add OpenAPI services for OAuth2 to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <param name="configuration">The configuration.</param>
    /// <param name="configurationSection">The configuration section for OAuth2OpenApi options.</param>
    /// <returns>The modified <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection TryAddJwtBearerOpenApi(
        this IServiceCollection services,
        IConfiguration configuration,
#if DEBUG
        string configurationSection
#else
        string configurationSection = nameof(OAuth2OpenApiOptions)
#endif
    )
    {
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigureOptions<OpenApiOptions>, ConfigureOAuthOpenApiOptions>());
        services.TryAddEnumerable(ServiceDescriptor.Singleton<IApiReferenceConfigurator, OAuthApiReferenceConfigurator>());

        services.Configure<OAuth2OpenApiOptions>(options => configuration.Bind(configurationSection, options));

        return services;
    }
}
