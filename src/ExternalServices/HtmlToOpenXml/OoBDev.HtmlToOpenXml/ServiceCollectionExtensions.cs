using OoBDev.Documents.Conversion;
using OoBDev.Documents.Models;
using Microsoft.Extensions.DependencyInjection;

namespace OoBDev.HtmlToOpenXml;

/// <summary>
/// Provides extension methods for configuring services related to WkHtmlToPdf.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Configures services for WkHtmlToPdf.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <returns>The modified <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection TryAddHtmlToOpenXmlServices(this IServiceCollection services)
    {
        //TODO: consider switching to TIKA or at the very lease making it so thsi can be excluded from common
        services.AddTransient<IDocumentConversionHandler, HtmlToDocxConversionHandler>();

        services.AddTransient<IDocumentType>(_ => new DocumentType
        {
            Name = "Microsoft Word - OpenXML",
            FileHeader = [],
            FileExtensions = [".docx",],
            ContentTypes = HtmlToDocxConversionHandler.DESTINATIONS,
        });

        return services;
    }
}

