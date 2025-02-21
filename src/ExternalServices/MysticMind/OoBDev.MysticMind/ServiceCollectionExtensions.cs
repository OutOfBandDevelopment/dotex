using OoBDev.Documents.Conversion;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace OoBDev.MysticMind;

/// <summary>
/// Provides extension methods for configuring services related to MysticMind.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Configures services for MysticMind.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <returns>The modified <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection TryAddMysticMindServices(this IServiceCollection services)
    {
        services.TryAddTransient<IConverterFactory, ConverterFactory>();
        services.TryAddTransient(sp => sp.GetRequiredService<IConverterFactory>().Build());
        services.AddTransient<IDocumentConversionHandler, HtmlToMarkdownConversionHandler>();

        return services;
    }
}
