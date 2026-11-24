using System;
using System.Threading.Tasks;
using OoBDev.Documents.Containers;
using OoBDev.Documents.Models;
namespace OoBDev.Documents.Store;

/// <summary>Picks the blob container by name from <see cref="IBlobContainerFactory"/> and delegates to a <see cref="DocumentStore"/>.</summary>
/// <param name="containers">Creates containers by name.</param>
/// <param name="keys">Makes the keys.</param>
/// <param name="validator">Checks content first.</param>
public sealed class DocumentResolver(IBlobContainerFactory containers, IDocumentKeyGenerator keys, IValidateContent validator) : IDocumentResolver
{
    /// <inheritdoc />
    public async Task<DocumentReference> StoreAsync(string container, ContentReference content)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(container);
        var key = await new DocumentStore(containers.Create(container), keys, validator).StoreAsync(content).ConfigureAwait(false);
        return new DocumentReference(container, key);
    }

    /// <inheritdoc />
    public Task<ContentReference?> ResolveAsync(DocumentReference reference)
    {
        ArgumentNullException.ThrowIfNull(reference);
        return containers.Create(reference.Container).GetContentAsync(reference.Key);
    }
}
