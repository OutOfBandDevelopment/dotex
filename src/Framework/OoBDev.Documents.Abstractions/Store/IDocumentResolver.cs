using System.Threading.Tasks;
using OoBDev.Documents.Models;
namespace OoBDev.Documents.Store;

/// <summary>Stores and reads documents in named containers, so a request can carry a <see cref="DocumentReference"/> instead of the bytes.</summary>
public interface IDocumentResolver
{
    /// <summary>Validates and stores the document in the named container.</summary>
    /// <param name="container">The blob container name.</param>
    /// <param name="content">The document.</param>
    /// <exception cref="InvalidContentException">Validation refused the content.</exception>
    Task<DocumentReference> StoreAsync(string container, ContentReference content);

    /// <summary>Reads the referenced document, or null when it is not there.</summary>
    /// <param name="reference">The reference.</param>
    Task<ContentReference?> ResolveAsync(DocumentReference reference);
}
