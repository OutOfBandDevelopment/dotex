using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OoBDev.Documents.Containers;

namespace OoBDev.Amazon.S3;

/// <summary>Creates <see cref="S3BlobContainerProvider"/> instances while S3 is configured.</summary>
/// <param name="serviceProvider">The service provider.</param>
/// <param name="options">The options.</param>
public sealed class S3BlobContainerProviderFactory(IServiceProvider serviceProvider, IOptions<S3BlobOptions> options) : IBlobContainerProviderFactory
{
    /// <inheritdoc />
    public IBlobContainerProvider? Create(string containerName)
    {
        var value = options.Value;
        return string.IsNullOrEmpty(value.Region) && string.IsNullOrEmpty(value.ServiceUrl)
            ? null
            : ActivatorUtilities.CreateInstance<S3BlobContainerProvider>(serviceProvider, containerName);
    }
}
