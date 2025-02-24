using OoBDev.Apache.Tika;
using OoBDev.Azure.StorageAccount;
using OoBDev.GroqCloud;
using OoBDev.Handlebars;
using OoBDev.HtmlToOpenXml;
using OoBDev.Keycloak;
using OoBDev.MailKit;
using OoBDev.Markdig;
using OoBDev.Microsoft.ApplicationInsights;
using OoBDev.Microsoft.B2C;
using OoBDev.MongoDB;
using OoBDev.MysticMind;
using OoBDev.Ollama;
using OoBDev.OpenSearch;
using OoBDev.Qdrant;
using OoBDev.RabbitMQ;
using OoBDev.SBert;
using OoBDev.WkHtmlToPdf;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OoBDev.SBert.AllMiniLML6v2Sharp;

namespace OoBDev.Common.Extensions;

/// <summary>
/// Provides extension methods for configuring common external services in the <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Tries to add common external services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> instance.</param>
    /// <param name="configuration">The configuration containing settings for external services.</param>
    /// <param name="identityBuilder">Optional builder for configuring identity extensions. Default is <c>null</c>.</param>
    /// <param name="externalBuilder">Optional builder for configuring external extensions. Default is <c>null</c>.</param>
    /// <returns>The updated <see cref="IServiceCollection"/> instance.</returns>
    public static IServiceCollection TryCommonExternalExtensions(
        this IServiceCollection services,
        IConfiguration configuration,
#if DEBUG
        IdentityExtensionBuilder? identityBuilder,
        ExternalExtensionBuilder? externalBuilder
#else
        IdentityExtensionBuilder? identityBuilder = default,
        ExternalExtensionBuilder? externalBuilder = default
#endif
    )
    {
        identityBuilder ??= new();
        externalBuilder ??= new();

        services.TryAddMongoServices(configuration, externalBuilder.MongoDatabaseConfigurationSection);
        services.TryAddAzureStorageServices(configuration, externalBuilder.AzureBlobProviderOptionSection);
        services.TryAddRabbitMQServices();
        services.TryAddMailKitExtensions(configuration, externalBuilder.SmtpConfigurationSection, externalBuilder.ImapConfigurationSection);
#if DEBUG
#warning Feature is not complete and should not be used in production.
        services.TryAddApplicationInsightsExtensions();
#endif

        if (identityBuilder.IdentityProvider.HasFlag(IdentityProviders.AzureB2C))
            services.TryAddMicrosoftB2CServices(configuration, identityBuilder.MicrosoftIdentityConfigurationSection);

        if (identityBuilder.IdentityProvider.HasFlag(IdentityProviders.Keycloak))
            services.TryAddKeycloakServices(configuration, identityBuilder.KeycloakIdentityConfigurationSection);

        services.TryAddSbertServices(configuration, externalBuilder.SentenceEmbeddingOptionSection);
        services.TryAddAllMiniLmL6V2Services(configuration, externalBuilder.SentenceEmbeddingOptionSection);
        services.TryAddQdrantServices(configuration, externalBuilder.QdrantOptionSection);

        services.TryAddOpenSearchServices(configuration, externalBuilder.OpenSearchOptionSection);
        services.TryAddOllamaServices(configuration, externalBuilder.OllamaApiClientOptionSection);
        services.TryAddGroqCloudServices(configuration, externalBuilder.GroqCloudApiClientOptionSection);

        services.TryAddApacheTikaServices(configuration, externalBuilder.ApacheTikaClientOptionSection);

        services.TryAddWkHtmlToPdfServices();
        services.TryAddMarkdigServices();
        services.TryAddHandlebarServices();
        services.TryAddMysticMindServices();
        services.TryAddHtmlToOpenXmlServices();

        return services;
    }
}
