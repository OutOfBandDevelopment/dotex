using System;
using System.Threading.Tasks;
using OoBDev.Documents.Models;
namespace OoBDev.Documents.Store;

/// <summary>Validates, names and stores documents in a blob container.</summary>
/// <param name="container">Where documents are kept.</param>
/// <param name="keys">Makes the keys.</param>
/// <param name="validator">Checks content first.</param>
public sealed class DocumentStore(IBlobContainer container, IDocumentKeyGenerator keys, IValidateContent validator) : IDocumentStore
{
    /// <inheritdoc />
    public async Task<string> StoreAsync(ContentReference content)
    {
        ArgumentNullException.ThrowIfNull(content);
        var start = content.Content.CanSeek ? content.Content.Position : 0;
        await validator.ValidateAsync(content).ConfigureAwait(false);
        if (content.Content.CanSeek)
        {
            content.Content.Position = start;
        }

        var key = keys.Create(content.FileName);
        await container.StoreContentAsync(content with { FileName = key }).ConfigureAwait(false);
        return key;
    }

    /// <inheritdoc />
    public Task<ContentReference?> ReadAsync(string key) => container.GetContentAsync(key);
}
