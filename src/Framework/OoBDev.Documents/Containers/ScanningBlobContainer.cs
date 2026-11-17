using OoBDev.Documents.Models;
using OoBDev.Documents.Scanning;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OoBDev.Documents.Containers;

/// <summary>
/// Scans content before it reaches the wrapped container and refuses it when the scan finds something
/// or fails (fail closed). Reads, queries and deletes pass straight through.
/// </summary>
public class ScanningBlobContainer(IBlobContainer wrapped, IContentScanner scanner) : IBlobContainer
{
    /// <inheritdoc/>
    public Task DeleteContentAsync(string path) => wrapped.DeleteContentAsync(path);

    /// <inheritdoc/>
    public Task<ContentReference?> GetContentAsync(string path) => wrapped.GetContentAsync(path);

    /// <inheritdoc/>
    public Task<ContentMetaDataReference?> GetContentMetaDataAsync(string path) => wrapped.GetContentMetaDataAsync(path);

    /// <inheritdoc/>
    public IQueryable<ContentMetaDataReference> QueryContent() => wrapped.QueryContent();

    /// <inheritdoc/>
    public Task<bool> StoreContentMetaDataAsync(ContentMetaDataReference reference) => wrapped.StoreContentMetaDataAsync(reference);

    /// <inheritdoc/>
    /// <exception cref="ContentRejectedException">The scan found something or could not complete.</exception>
    public async Task StoreContentAsync(ContentReference reference, IDictionary<string, string>? metadata = null, bool overwrite = false)
    {
        ArgumentNullException.ThrowIfNull(reference);

        var content = reference;
        if (!reference.Content.CanSeek)
        {
            var buffer = new MemoryStream();
            await reference.Content.CopyToAsync(buffer).ConfigureAwait(false);
            content = reference with { Content = buffer };
        }

        ContentScanResult result;
        try
        {
            content.Content.Position = 0;
            result = await scanner.ScanAsync(content).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not ContentRejectedException)
        {
            throw new ContentRejectedException($"The scan of '{reference.FileName}' could not complete, so it was not stored.", ex);
        }

        if (!result.IsClean)
        {
            throw new ContentRejectedException($"'{reference.FileName}' was refused: {result.Detail ?? "the scan reported a problem"}.");
        }

        content.Content.Position = 0;
        await wrapped.StoreContentAsync(content, metadata, overwrite).ConfigureAwait(false);
    }
}
