using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using OoBDev.Documents.Containers;
using OoBDev.Documents.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OoBDev.Microsoft.Azure.StorageAccount.BlobStorage;

/// <summary>
/// Represents a provider for storing and searching content in Azure Blob storage.
/// </summary>
public partial class AzureBlobContainerProvider : IBlobContainerProvider
{
    private readonly BlobContainerClient _blockBlobClient;
    private readonly IOptions<AzureBlobProviderOptions> _options;
    private readonly ILogger _logger;

    /// <summary>
    /// Container name for this instance
    /// </summary>
    public string ContainerName { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="AzureBlobContainerProvider"/> class with the specified <paramref name="client"/> and <paramref name="collectionName"/>.
    /// </summary>
    /// <param name="client">The BlobServiceClient used to connect to the Azure Blob storage.</param>
    /// <param name="collectionName">The name of the collection in the Azure Blob storage.</param>
    /// <param name="options">The configuration options to the azure container service.</param>
    /// <param name="loggerFactory">ILoggerFactory instance.</param>
    public AzureBlobContainerProvider(
        BlobServiceClient client,
        string collectionName,
        IOptions<AzureBlobProviderOptions> options,
        ILoggerFactory loggerFactory
        )
    {
        _logger = loggerFactory.CreateLogger(nameof(AzureBlobContainerProvider) + $"-{collectionName}");
        ContainerName = collectionName.ToLowerInvariant();
        _blockBlobClient = client.GetBlobContainerClient(ContainerName);
        _options = options;
    }

    private async Task EnsureContainerExistsAsync()
    {
        if (_options.Value.EnsureContainerExists)
            await _blockBlobClient.CreateIfNotExistsAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// Retrieves the content of the specified file from Azure Blob storage.
    /// </summary>
    /// <param name="file">The name of the file to retrieve.</param>
    /// <returns>A ContentReference object representing the retrieved content.</returns>
    public async Task<ContentReference?> GetContentAsync(string file)
    {
        await EnsureContainerExistsAsync().ConfigureAwait(false);
        var blob = _blockBlobClient.GetBlobClient(file);

        if (!await blob.ExistsAsync().ConfigureAwait(false))
            return null;

        var result = await blob.DownloadStreamingAsync().ConfigureAwait(false);
        return new ContentReference
        {
            Content = result.Value.Content,
            ContentType = result.Value.Details.ContentType,
            FileName = Path.GetFileName(file)
        };
    }

    /// <summary>
    /// Retrieves content metadata asynchronously.
    /// </summary>
    /// <param name="path">The path to the content.</param>
    /// <returns>A task representing the asynchronous operation. Returns the content metadata if it exists, otherwise returns null.</returns>
    public async Task<ContentMetaDataReference?> GetContentMetaDataAsync(string path)
    {
        await EnsureContainerExistsAsync().ConfigureAwait(false);
        var blob = _blockBlobClient.GetBlobClient(path);
        if (!await blob.ExistsAsync().ConfigureAwait(false)) return null;

        var header = await blob.GetPropertiesAsync().ConfigureAwait(false);
        return new ContentMetaDataReference
        {
            FileName = blob.Name,
            ContentType = header.Value.ContentType,
            MetaData = header.Value.Metadata.ToDictionary(k => k.Key, k => k.Value),
        };
    }

    /// <summary>
    /// Stores content asynchronously.
    /// </summary>
    /// <param name="reference">The reference to the content.</param>
    /// <param name="metadata">The metadata associated with the content.</param>
    /// <param name="overwrite">Determines whether to overwrite existing content with the same name.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task StoreContentAsync(
        ContentReference reference,
        Dictionary<string, string>? metadata = null,
        bool overwrite = false
        )
    {
        await EnsureContainerExistsAsync().ConfigureAwait(false);
        var blob = _blockBlobClient.GetBlobClient(reference.FileName);

        LogUpload(reference.FileName);
        _ = await blob.UploadAsync(reference.Content, overwrite: overwrite).ConfigureAwait(false);

        _ = await blob.SetHttpHeadersAsync(new BlobHttpHeaders
        {
            ContentType = reference.ContentType,
        }).ConfigureAwait(false);

        // https://learn.microsoft.com/en-us/azure/storage/blobs/storage-blob-properties-metadata

        if (metadata != null)
        {
            var data = metadata.ToDictionary(k => k.Key, v => $"{v.Value}");
            await blob.SetMetadataAsync(data).ConfigureAwait(false);
        }
    }

    /// <summary>
    /// Stores content metadata asynchronously.
    /// </summary>
    /// <param name="reference">The reference to the content metadata.</param>
    /// <returns>A task representing the asynchronous operation. Returns true if the metadata is stored successfully, otherwise false.</returns>
    public async Task<bool> StoreContentMetaDataAsync(ContentMetaDataReference reference)
    {
        await EnsureContainerExistsAsync().ConfigureAwait(false);
        var blob = _blockBlobClient.GetBlobClient(reference.FileName);
        if (!await blob.ExistsAsync().ConfigureAwait(false)) return false;

        LogUpdate(reference.FileName);

        _ = await blob.SetHttpHeadersAsync(new BlobHttpHeaders
        {
            ContentType = reference.ContentType,
        }).ConfigureAwait(false);

        await blob.SetMetadataAsync(reference.MetaData).ConfigureAwait(false);

        return true;
    }

    /// <summary>
    /// Queries content metadata.
    /// </summary>
    /// <returns>An IQueryable representing the content metadata.</returns>
    public IQueryable<ContentMetaDataReference> QueryContent()
    {
        EnsureContainerExistsAsync().GetAwaiter().GetResult();
        //TODO: build a query provider!
        var query = from p in _blockBlobClient.GetBlobs().AsPages()
                    from b in p.Values
                    select new ContentMetaDataReference
                    {
                        FileName = b.Name,
                        ContentType = b.Properties.ContentType,
                        MetaData = b.Metadata.ToDictionary(k => k.Key, k => k.Value),
                    };
        return query.AsQueryable();
    }

    /// <summary>
    /// Deletes content asynchronously.
    /// </summary>
    /// <param name="path">The path to the content to be deleted.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public async Task DeleteContentAsync(string path)
    {
        await EnsureContainerExistsAsync().ConfigureAwait(false);
        var blob = _blockBlobClient.GetBlobClient(path);
        await blob.DeleteIfExistsAsync().ConfigureAwait(false);
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "upload -> {file}")]
    private partial void LogUpload(string file);

    [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "update -> {file}")]
    private partial void LogUpdate(string file);
}

//public class BlobQueryProvider : IQueryProvider
//{
//    // https://learn.microsoft.com/en-us/previous-versions/bb546158(v=vs.140)?redirectedfrom=MSDN
//    public IQueryable CreateQuery(Expression expression) => throw new NotImplementedException();
//    public IQueryable<TElement> CreateQuery<TElement>(Expression expression) => throw new NotImplementedException();
//    public object? Execute(Expression expression) => throw new NotImplementedException();
//    public TResult Execute<TResult>(Expression expression) => throw new NotImplementedException();
//}
