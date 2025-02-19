namespace OoBDev.Azure.StorageAccount.BlobStorage;

/// <summary>
/// Options for configuring Azure Blob storage provider.
/// </summary>
public class AzureBlobProviderOptions
{
    /// <summary>
    /// Gets or sets the connection string for Azure Blob storage.
    /// </summary>
    public required string ConnectionString { get; set; }

    /// <summary>
    /// if true the system will create a container if not exists
    /// </summary>
    public bool EnsureContainerExists { get; set; }
}
