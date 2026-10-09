using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace OoBDev.System;

/// <summary>
/// Shared options registration with validation.
/// </summary>
public static class OptionsServiceCollectionExtensions
{
    /// <summary>
    /// Binds <typeparamref name="TOptions"/> to a configuration section and validates it with data annotations.
    /// </summary>
    /// <typeparam name="TOptions">The options type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration to bind from.</param>
    /// <param name="configurationSection">The section name (usually <c>nameof(TOptions)</c>).</param>
    /// <param name="validateOnStart">When true (strict, the default) invalid options fail host startup; when false they fail on first use.</param>
    /// <returns>The <see cref="OptionsBuilder{TOptions}"/> for further configuration.</returns>
    public static OptionsBuilder<TOptions> AddValidatedOptions<TOptions>(
        this IServiceCollection services,
        IConfiguration configuration,
        string configurationSection,
        bool validateOnStart = true
        )
        where TOptions : class
    {
        var builder = services
            .AddOptions<TOptions>()
            .Bind(configuration.GetSection(configurationSection))
            .ValidateDataAnnotations();
        if (validateOnStart)
            builder.ValidateOnStart();
        return builder;
    }
}
