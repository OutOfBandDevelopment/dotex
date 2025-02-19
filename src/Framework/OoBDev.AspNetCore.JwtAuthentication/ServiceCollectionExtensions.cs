using OoBDev.AspNetCore.JwtAuthentication.SwaggerGen;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.SwaggerGen;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace OoBDev.AspNetCore.JwtAuthentication;

/// <summary>
/// Extension methods for configuring JWT Bearer authentication and SwaggerGen services in <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Tries to add JWT Bearer authentication and SwaggerGen services to the specified <see cref="IServiceCollection"/>.
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
        services.TryAddJwtBearerSwaggerGen(configuration, builder.OAuth2SwaggerConfigurationSection);
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
    /// Tries to add SwaggerGen services for OAuth2 to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <param name="configuration">The configuration.</param>
    /// <param name="configurationSection">The configuration section for OAuth2Swagger options.</param>
    /// <returns>The modified <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection TryAddJwtBearerSwaggerGen(
        this IServiceCollection services,
        IConfiguration configuration,
#if DEBUG
        string configurationSection
#else
        string configurationSection = nameof(OAuth2SwaggerOptions)
#endif
    )
    {
        services.AddSingleton<IConfigureOptions<SwaggerUIOptions>, ConfigureOAuthSwaggerUIOptions>();
        services.AddSingleton<IConfigureOptions<SwaggerGenOptions>, ConfigureOAuthSwaggerGenOptions>();

        services.Configure<OAuth2SwaggerOptions>(options => configuration.Bind(configurationSection, options));

        return services;
    }
}
