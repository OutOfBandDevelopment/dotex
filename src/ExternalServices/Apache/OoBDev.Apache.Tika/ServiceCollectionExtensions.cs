using OoBDev.Apache.Tika.Detectors;
using OoBDev.Apache.Tika.Handlers;
using OoBDev.Documents;
using OoBDev.Documents.Conversion;
using OoBDev.Documents.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Apache.Tika;

/// <summary>
/// Provides extension methods for configuring services related to Apache - Tika.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Configures services for Apache - Tika.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <returns>The modified <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection TryAddApacheTikaServices(
        this IServiceCollection services,
        IConfiguration configuration,
#if DEBUG
        string apacheTikaClientOptionSection
#else
        string apacheTikaClientOptionSection = nameof(ApacheTikaClientOptions)
#endif
        )
    {
        var url = configuration.GetSection(apacheTikaClientOptionSection)?[nameof(ApacheTikaClientOptions.Url)];
        if (url == null)
        {
            return services;
        }
        services.AddHealthChecks().AddCheck<ApacheTikaHealthCheck>("apache-tika");

        services.Configure<ApacheTikaClientOptions>(options => configuration.Bind(apacheTikaClientOptionSection, options));

        services.AddHttpClient<IApacheTikaClient, ApacheTikaClient>((sp, http) =>
        {
            var options = sp.GetRequiredService<IOptions<ApacheTikaClientOptions>>();
            http.BaseAddress = new Uri(options.Value.Url);
        });

        services.AddTransient<IContentTypeDetector, TikaContentTypeDetector>();

        services.AddTransient<IDocumentConversionHandler, TikaDocToHtmlConversionHandler>();
        services.AddTransient<IDocumentConversionHandler, TikaDocxToHtmlConversionHandler>();
        services.AddTransient<IDocumentConversionHandler, TikaEpubToHtmlConversionHandler>();
        services.AddTransient<IDocumentConversionHandler, TikaOdtToHtmlConversionHandler>();
        services.AddTransient<IDocumentConversionHandler, TikaPdfToHtmlConversionHandler>();
        services.AddTransient<IDocumentConversionHandler, TikaRtfToHtmlConversionHandler>();

        services.AddTransient<IDocumentType>(_ => new DocumentType
        {
            Name = "Microsoft Word",
            FileHeader = [],
            FileExtensions = [".doc",],
            ContentTypes = TikaDocToHtmlConversionHandler.SOURCES,
        });
        services.AddTransient<IDocumentType>(_ => new DocumentType
        {
            Name = "Microsoft Word - OpenXML",
            FileHeader = [],
            FileExtensions = [".docx",],
            ContentTypes = TikaDocxToHtmlConversionHandler.SOURCES,
        });
        services.AddTransient<IDocumentType>(_ => new DocumentType
        {
            Name = "Portable Document Format",
            FileHeader = [(byte)'%', (byte)'P', (byte)'D', (byte)'F', (byte)'-'],
            FileExtensions = [".pdf",],
            ContentTypes = TikaPdfToHtmlConversionHandler.SOURCES,
        });
        services.AddTransient<IDocumentType>(_ => new DocumentType
        {
            Name = "Open Office Document",
            FileHeader = [],
            FileExtensions = [".odt",],
            ContentTypes = TikaOdtToHtmlConversionHandler.SOURCES,
        });
        services.AddTransient<IDocumentType>(_ => new DocumentType
        {
            Name = "Electronic publication",
            FileHeader = [],
            FileExtensions = [".epub",],
            ContentTypes = TikaEpubToHtmlConversionHandler.SOURCES,
        });
        services.AddTransient<IDocumentType>(_ => new DocumentType
        {
            Name = "Rich Text Format",
            FileHeader = [],
            FileExtensions = [".rtf",],
            ContentTypes = TikaRtfToHtmlConversionHandler.SOURCES,
        });

        return services;
    }
}
