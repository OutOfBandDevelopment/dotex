using Azure.Storage.Blobs;
using Microsoft.Extensions.Options;

namespace OoBDev.Azure.StorageAccount.BlobStorage;

/// <summary>
/// Represents a factory for creating instances of <see cref="BlobServiceClient"/>.
/// </summary>
public class AzureBlobServiceClientFactory : IBlobServiceClientFactory
{
    private readonly IOptions<AzureBlobProviderOptions> _config;

    /// <summary>
    /// Initializes a new instance of the <see cref="AzureBlobServiceClientFactory"/> class with the specified configuration.
    /// </summary>
    /// <param name="config">The Azure Blob provider options.</param>
    public AzureBlobServiceClientFactory(
        IOptions<AzureBlobProviderOptions> config
    ) => _config = config;

    /// <summary>
    /// Creates a new instance of <see cref="BlobServiceClient"/>.
    /// </summary>
    /// <returns>A new instance of <see cref="BlobServiceClient"/>.</returns>
    public BlobServiceClient Create() => new(_config.Value.ConnectionString);
}
