using OoBDev.Documents.Conversion;
using OoBDev.Documents.Models;
using Markdig;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace OoBDev.Markdig;

/// <summary>
/// Provides extension methods for configuring services related to Markdig.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Configures services for Markdig.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <returns>The modified <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection TryAddMarkdigServices(this IServiceCollection services)
    {
        services.TryAddTransient(_ => new MarkdownPipelineBuilder().UseAdvancedExtensions().Build());

        services.AddTransient<IDocumentConversionHandler, MarkdownToHtmlConversionHandler>();

        services.AddTransient<IDocumentType>(_ => new DocumentType
        {
            Name = "Markdown",
            FileHeader = [],
            FileExtensions = [".md",],
            ContentTypes = ["text/markdown", "text/x-markdown"],
        });

        return services;
    }
}
