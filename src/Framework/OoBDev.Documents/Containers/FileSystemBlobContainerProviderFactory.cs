using Microsoft.Extensions.Options;

namespace OoBDev.Documents.Containers;

/// <summary>Creates <see cref="FileSystemBlobContainerProvider"/> instances while a root directory is configured.</summary>
/// <param name="options">The options.</param>
public sealed class FileSystemBlobContainerProviderFactory(IOptions<FileSystemBlobOptions> options) : IBlobContainerProviderFactory
{
    /// <inheritdoc />
    public IBlobContainerProvider? Create(string containerName) =>
        string.IsNullOrWhiteSpace(options.Value.RootDirectory) ? null : new FileSystemBlobContainerProvider(options.Value.RootDirectory, containerName);
}
