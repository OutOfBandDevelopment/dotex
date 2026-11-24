using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Util;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OoBDev.Documents.Containers;
using OoBDev.Documents.Models;

namespace OoBDev.Amazon.S3;

/// <summary>An <see cref="IBlobContainerProvider"/> over one S3 bucket; the container name is the bucket name and a path is the object key.</summary>
public partial class S3BlobContainerProvider : IBlobContainerProvider
{
    private const string _metadataPrefix = "x-amz-meta-";

    private readonly IAmazonS3 _client;
    private readonly IOptions<S3BlobOptions> _options;
    private readonly ILogger _logger;

    /// <summary>Creates the provider for <paramref name="containerName"/>.</summary>
    /// <param name="client">The S3 client.</param>
    /// <param name="containerName">The bucket name.</param>
    /// <param name="options">The options.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    public S3BlobContainerProvider(IAmazonS3 client, string containerName, IOptions<S3BlobOptions> options, ILoggerFactory loggerFactory)
    {
        _client = client;
        _options = options;
        ContainerName = containerName.ToLowerInvariant();
        _logger = loggerFactory.CreateLogger(nameof(S3BlobContainerProvider) + $"-{containerName}");
    }

    /// <inheritdoc />
    public string ContainerName { get; set; }

    /// <inheritdoc />
    public async Task<ContentReference?> GetContentAsync(string path)
    {
        await EnsureBucketAsync().ConfigureAwait(false);
        try
        {
            var response = await _client.GetObjectAsync(ContainerName, path).ConfigureAwait(false);
            return new ContentReference
            {
                Content = response.ResponseStream,
                ContentType = response.Headers.ContentType,
                FileName = Path.GetFileName(path),
            };
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<ContentMetaDataReference?> GetContentMetaDataAsync(string path)
    {
        await EnsureBucketAsync().ConfigureAwait(false);
        try
        {
            var response = await _client.GetObjectMetadataAsync(ContainerName, path).ConfigureAwait(false);
            return new ContentMetaDataReference
            {
                FileName = path,
                ContentType = response.Headers.ContentType,
                MetaData = ReadMetadata(response.Metadata),
            };
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
    }

    /// <inheritdoc />
    public async Task StoreContentAsync(ContentReference reference, IDictionary<string, string>? metadata = null, bool overwrite = false)
    {
        ArgumentNullException.ThrowIfNull(reference);
        await EnsureBucketAsync().ConfigureAwait(false);
        if (!overwrite && await ExistsAsync(reference.FileName).ConfigureAwait(false))
        {
            throw new IOException($"'{reference.FileName}' already exists in '{ContainerName}' and overwrite is off.");
        }

        var request = new PutObjectRequest
        {
            BucketName = ContainerName,
            Key = reference.FileName,
            InputStream = reference.Content,
            ContentType = reference.ContentType,
            AutoCloseStream = false,
        };
        AddMetadata(request.Metadata, metadata);

        LogUpload(reference.FileName);
        _ = await _client.PutObjectAsync(request).ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<bool> StoreContentMetaDataAsync(ContentMetaDataReference reference)
    {
        ArgumentNullException.ThrowIfNull(reference);
        await EnsureBucketAsync().ConfigureAwait(false);
        if (!await ExistsAsync(reference.FileName).ConfigureAwait(false))
        {
            return false;
        }

        LogUpdate(reference.FileName);
        var request = new CopyObjectRequest
        {
            SourceBucket = ContainerName,
            SourceKey = reference.FileName,
            DestinationBucket = ContainerName,
            DestinationKey = reference.FileName,
            MetadataDirective = S3MetadataDirective.REPLACE,
            ContentType = reference.ContentType,
        };
        AddMetadata(request.Metadata, reference.MetaData);
        _ = await _client.CopyObjectAsync(request).ConfigureAwait(false);
        return true;
    }

    /// <summary>Queries content metadata. Objects are listed page by page with one metadata request each as the query is enumerated; filters run in memory over that listing.</summary>
    /// <returns>The content metadata.</returns>
    public IQueryable<ContentMetaDataReference> QueryContent()
    {
        EnsureBucketAsync().GetAwaiter().GetResult();
        return ListKeys().Select(key => GetContentMetaDataAsync(key).GetAwaiter().GetResult()).OfType<ContentMetaDataReference>().AsQueryable();
    }

    /// <inheritdoc />
    public async Task DeleteContentAsync(string path)
    {
        await EnsureBucketAsync().ConfigureAwait(false);
        _ = await _client.DeleteObjectAsync(ContainerName, path).ConfigureAwait(false);
    }

    private IEnumerable<string> ListKeys()
    {
        string? token = null;
        do
        {
            var page = _client.ListObjectsV2Async(new ListObjectsV2Request { BucketName = ContainerName, ContinuationToken = token }).GetAwaiter().GetResult();
            foreach (var item in page.S3Objects ?? [])
            {
                yield return item.Key;
            }

            token = page.IsTruncated == true ? page.NextContinuationToken : null;
        }
        while (token is not null);
    }

    private async Task<bool> ExistsAsync(string key)
    {
        try
        {
            _ = await _client.GetObjectMetadataAsync(ContainerName, key).ConfigureAwait(false);
            return true;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }
    }

    private async Task EnsureBucketAsync()
    {
        if (_options.Value.EnsureContainerExists && !await AmazonS3Util.DoesS3BucketExistV2Async(_client, ContainerName).ConfigureAwait(false))
        {
            _ = await _client.PutBucketAsync(ContainerName).ConfigureAwait(false);
        }
    }

    private static void AddMetadata(MetadataCollection target, IDictionary<string, string>? metadata)
    {
        foreach (var (key, value) in metadata ?? new Dictionary<string, string>(StringComparer.Ordinal))
        {
            target[key] = value;
        }
    }

    private static Dictionary<string, string> ReadMetadata(MetadataCollection metadata) =>
        metadata.Keys.ToDictionary(k => k.StartsWith(_metadataPrefix, StringComparison.OrdinalIgnoreCase) ? k[_metadataPrefix.Length..] : k, k => metadata[k], StringComparer.Ordinal);

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "upload -> {file}")]
    private partial void LogUpload(string file);

    [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "update -> {file}")]
    private partial void LogUpdate(string file);
}
