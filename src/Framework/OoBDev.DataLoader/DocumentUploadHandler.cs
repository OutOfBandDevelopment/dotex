using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using OoBDev.Documents.Models;
using OoBDev.Documents.Store;

namespace OoBDev.DataLoader;

/// <summary>Uploads the file a row names to an <see cref="IDocumentStore"/> and stores the key in the row (the source <c>DocumentUpload</c> handler).</summary>
/// <param name="priority">The pipeline priority.</param>
/// <param name="files">Resolves the file names against the data folder.</param>
/// <param name="store">Where the document goes.</param>
/// <param name="options">Column names; the defaults match the source.</param>
public sealed class DocumentUploadHandler(int priority, SourceFileResolver files, IDocumentStore store, DocumentUploadOptions? options = null) : IPipelineHandler
{
    private readonly DocumentUploadOptions settings = options ?? new DocumentUploadOptions();

    /// <inheritdoc />
    public int Priority => priority;

    /// <inheritdoc />
    public async ValueTask<RowOutcome> HandleAsync(RowContext row, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(row);
        row.Values.TryGetValue(settings.FileColumn, out var name);
        row.Values.TryGetValue(settings.ContentTypeColumn, out var contentType);
        row.Values.Remove(settings.FileColumn);
        row.Values.Remove(settings.ContentTypeColumn);

        var alreadyStored = row.Values.TryGetValue(settings.KeyColumn, out var existing) && !string.IsNullOrWhiteSpace(existing);
        if (string.IsNullOrWhiteSpace(name) || alreadyStored)
        {
            return RowOutcome.Accept;
        }

        if (!files.TryResolve(name, out var path, out var error))
        {
            return RowOutcome.Reject($"'{settings.FileColumn}': {error}");
        }

        var content = File.OpenRead(path);
        await using var _ = content.ConfigureAwait(false);
        var reference = new ContentReference
        {
            Content = content,
            ContentType = string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : contentType,
            FileName = Path.GetFileName(path),
        };

        try
        {
            row.Values[settings.KeyColumn] = await store.StoreAsync(reference).ConfigureAwait(false);
        }
        catch (InvalidContentException ex)
        {
            return RowOutcome.Reject($"'{settings.FileColumn}': {ex.Message}");
        }

        if (settings.NameColumn is { } nameColumn && (!row.Values.TryGetValue(nameColumn, out var current) || string.IsNullOrWhiteSpace(current)))
        {
            row.Values[nameColumn] = reference.FileName;
        }

        return RowOutcome.Accept;
    }
}
