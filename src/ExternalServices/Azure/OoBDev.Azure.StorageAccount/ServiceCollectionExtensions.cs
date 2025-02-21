using OoBDev.Azure.StorageAccount.BlobStorage;
using OoBDev.Azure.StorageAccount.MessageQueueing;
using OoBDev.Documents.Containers;
using OoBDev.MessageQueueing.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace OoBDev.Azure.StorageAccount;

/// <summary>
/// Provides extension methods for configuring Azure Storage services in the <see cref="IServiceCollection"/>.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Tries to add Azure Storage services including blob and queue services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="configuration">The <see cref="IConfiguration"/> to add services to.</param>
    /// <param name="azureBlobProviderOptionSection">The name for the ConfigurationSectionName.</param>
    /// <returns>The modified <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection TryAddAzureStorageServices(
        this IServiceCollection services,
        IConfiguration configuration,
#if DEBUG
        string azureBlobProviderOptionSection
#else
        string azureBlobProviderOptionSection = nameof(AzureBlobProviderOptions)
#endif
        ) =>
        services
            .TryAddAzureStorageBlobServices(configuration, azureBlobProviderOptionSection)
            .TryAddAzureStorageQueueServices()
        ;

    /// <summary>
    /// Configures services for Azure Storage Blob.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add the services to.</param>
    /// <param name="configuration">The <see cref="IConfiguration"/> to bind Azure Blob Storage options from.</param>
    /// <param name="azureBlobProviderOptionSection">The configuration section name containing Azure Blob Storage options.</param>
    /// <returns>The modified <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection TryAddAzureStorageBlobServices(
        this IServiceCollection services,
        IConfiguration configuration,
#if DEBUG
        string azureBlobProviderOptionSection
#else
        string azureBlobProviderOptionSection = nameof(AzureBlobProviderOptions)
#endif
        )
    {
        services.Configure<AzureBlobProviderOptions>(options => configuration.Bind(azureBlobProviderOptionSection, options));
        services.TryAddTransient<IBlobServiceClientFactory, AzureBlobServiceClientFactory>();
        services.TryAddTransient(sp => sp.GetRequiredService<IBlobServiceClientFactory>().Create());

        services.AddTransient<IBlobContainerProviderFactory, AzureBlobContainerProviderFactory>();

        return services;
    }

    /// <summary>
    /// Tries to add Azure Storage queue services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <returns>The modified <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection TryAddAzureStorageQueueServices(this IServiceCollection services)
    {
        services.AddTransient<IMessageSenderProvider, AzureStorageQueueMessageProvider>();
        services.TryAddKeyedTransient<IMessageSenderProvider, AzureStorageQueueMessageProvider>(AzureStorageGlobals.MessageProviderKey);

        services.AddTransient<IMessageReceiverProvider, AzureStorageQueueMessageProvider>();
        services.TryAddKeyedTransient<IMessageReceiverProvider, AzureStorageQueueMessageProvider>(AzureStorageGlobals.MessageProviderKey);

        services.TryAddTransient<IQueueClientFactory, QueueClientFactory>();
        services.TryAddTransient<IAzureStorageQueueMapper, AzureStorageQueueMapper>();

        return services;
    }
}
