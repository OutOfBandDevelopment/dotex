using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using OoBDev.Documents.Models;

namespace OoBDev.Documents.Containers;

/// <summary>
/// An <see cref="IBlobContainerProvider"/> over a directory: the container is a sub directory of the root and a path is a file below it.
/// The content type and metadata live in a <c>.blobmeta.json</c> file beside each file. Paths that leave the container directory are rejected.
/// </summary>
public sealed class FileSystemBlobContainerProvider : IBlobContainerProvider
{
    /// <summary>Extension of the file that holds the content type and metadata.</summary>
    public const string MetaExtension = ".blobmeta.json";

    private const string _defaultContentType = "application/octet-stream";

    private readonly string _root;
    private string _containerName;

    /// <summary>Creates the provider.</summary>
    /// <param name="rootDirectory">The root directory.</param>
    /// <param name="containerName">The container (sub directory) name.</param>
    public FileSystemBlobContainerProvider(string rootDirectory, string containerName)
    {
        _root = Path.GetFullPath(rootDirectory);
        _containerName = containerName;
        _ = ContainerDirectory();
    }

    /// <inheritdoc />
    public string ContainerName
    {
        get => _containerName;
        set
        {
            _containerName = value;
            _ = ContainerDirectory();
        }
    }

    /// <inheritdoc />
    public Task<ContentReference?> GetContentAsync(string path)
    {
        var file = Resolve(path);
        if (!File.Exists(file))
        {
            return Task.FromResult<ContentReference?>(null);
        }

        var meta = ReadMeta(file);
        return Task.FromResult<ContentReference?>(new ContentReference
        {
            Content = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous),
            ContentType = meta.ContentType,
            FileName = Path.GetFileName(file),
        });
    }

    /// <inheritdoc />
    public Task<ContentMetaDataReference?> GetContentMetaDataAsync(string path)
    {
        var file = Resolve(path);
        return Task.FromResult(File.Exists(file) ? ToReference(path, file) : null);
    }

    /// <inheritdoc />
    public async Task StoreContentAsync(ContentReference reference, IDictionary<string, string>? metadata = null, bool overwrite = false)
    {
        ArgumentNullException.ThrowIfNull(reference);
        var file = Resolve(reference.FileName);
        _ = Directory.CreateDirectory(Path.GetDirectoryName(file)!);
        var stream = new FileStream(file, overwrite ? FileMode.Create : FileMode.CreateNew, FileAccess.Write, FileShare.None, 4096, FileOptions.Asynchronous);
        await using (stream.ConfigureAwait(false))
        {
            await reference.Content.CopyToAsync(stream).ConfigureAwait(false);
        }

        WriteMeta(file, reference.ContentType, metadata);
    }

    /// <inheritdoc />
    public Task<bool> StoreContentMetaDataAsync(ContentMetaDataReference reference)
    {
        ArgumentNullException.ThrowIfNull(reference);
        var file = Resolve(reference.FileName);
        if (!File.Exists(file))
        {
            return Task.FromResult(false);
        }

        WriteMeta(file, reference.ContentType, reference.MetaData);
        return Task.FromResult(true);
    }

    /// <inheritdoc />
    public IQueryable<ContentMetaDataReference> QueryContent()
    {
        var directory = ContainerDirectory();
        return Directory.EnumerateFiles(directory, "*", SearchOption.AllDirectories)
            .Where(f => !f.EndsWith(MetaExtension, StringComparison.OrdinalIgnoreCase))
            .Select(f => ToReference(Path.GetRelativePath(directory, f).Replace('\\', '/'), f))
            .AsQueryable();
    }

    /// <inheritdoc />
    public Task DeleteContentAsync(string path)
    {
        var file = Resolve(path);
        File.Delete(file);
        File.Delete(file + MetaExtension);
        return Task.CompletedTask;
    }

    private string ContainerDirectory()
    {
        var directory = Path.GetFullPath(Path.Combine(_root, _containerName));
        if (!IsInside(_root, directory))
        {
            throw new InvalidOperationException($"Container '{_containerName}' is not a sub directory of the root.");
        }

        _ = Directory.CreateDirectory(directory);
        return directory;
    }

    private string Resolve(string path)
    {
        var directory = ContainerDirectory();
        var file = Path.GetFullPath(Path.Combine(directory, path));
        if (!IsInside(directory, file) || file.EndsWith(MetaExtension, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException($"Path '{path}' is not a blob path inside container '{_containerName}'.", nameof(path));
        }

        return file;
    }

    private static bool IsInside(string directory, string candidate) =>
        candidate.StartsWith(directory.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase);

    private static ContentMetaDataReference ToReference(string path, string file)
    {
        var meta = ReadMeta(file);
        return new ContentMetaDataReference { FileName = path, ContentType = meta.ContentType, MetaData = meta.MetaData };
    }

    private static Meta ReadMeta(string file)
    {
        var metaFile = file + MetaExtension;
        var meta = File.Exists(metaFile) ? JsonSerializer.Deserialize<Meta>(File.ReadAllText(metaFile)) : null;
        return meta ?? new Meta(_defaultContentType, new Dictionary<string, string>(StringComparer.Ordinal));
    }

    private static void WriteMeta(string file, string contentType, IDictionary<string, string>? metadata) =>
        File.WriteAllText(file + MetaExtension, JsonSerializer.Serialize(new Meta(contentType, new Dictionary<string, string>(metadata ?? new Dictionary<string, string>(StringComparer.Ordinal), StringComparer.Ordinal))));

    private sealed record Meta(string ContentType, Dictionary<string, string> MetaData);
}
