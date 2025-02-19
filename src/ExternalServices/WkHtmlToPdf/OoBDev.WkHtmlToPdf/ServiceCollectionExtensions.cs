using OoBDev.Documents.Conversion;
using OoBDev.Documents.Models;
using Microsoft.Extensions.DependencyInjection;
using WkHtmlToPdfDotNet;
using WkHtmlToPdfDotNet.Contracts;

namespace OoBDev.WkHtmlToPdf;

/// <summary>
/// Provides extension methods for configuring services related to WkHtmlToPdf.
/// </summary>
public static class ServiceCollectionExtensions
{
    private static IConverter? _converter;

    /// <summary>
    /// Configures services for WkHtmlToPdf.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <returns>The modified <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection TryAddWkHtmlToPdfServices(this IServiceCollection services)
    {
        //TODO: consider switching to TIKA or at the very lease making it so thsi can be excluded from common
        services.AddTransient<IDocumentConversionHandler, HtmlToPdfConversionHandler>();
        services.AddTransient<ITools, PdfTools>();
        services.AddSingleton<IConverter>(sp => _converter ??= ActivatorUtilities.CreateInstance<SynchronizedConverter>(sp));

        services.AddTransient<IDocumentType>(_ => new DocumentType
        {
            Name = "Portable Document Format",
            FileHeader = [(byte)'%', (byte)'P', (byte)'D', (byte)'F', (byte)'-'],
            FileExtensions = [".pdf",],
            ContentTypes = ["application/pdf"],
        });

        return services;
    }
}

