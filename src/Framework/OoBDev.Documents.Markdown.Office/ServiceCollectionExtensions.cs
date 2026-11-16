using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OoBDev.Documents.Markdown;

namespace OoBDev.Documents.Markdown.Office;

/// <summary>Registration for the office Markdown reader.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds <see cref="OfficeFileMarkdownReader"/> to the readers <c>DocumentMarkdownConverter</c> tries. It needs an
    /// <see cref="IDocumentConversion"/> whose chain reaches Markdown (for example the Tika and MysticMind handlers).
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The same collection.</returns>
    public static IServiceCollection TryAddOfficeMarkdown(this IServiceCollection services)
    {
        services.TryAddEnumerable(ServiceDescriptor.Transient<IFileMarkdownReader, OfficeFileMarkdownReader>());
        return services;
    }
}
