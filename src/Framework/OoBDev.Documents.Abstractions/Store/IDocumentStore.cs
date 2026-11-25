using System.Threading.Tasks;
using OoBDev.Documents.Models;
namespace OoBDev.Documents.Store;

/// <summary>Stores documents under generated keys, after validation.</summary>
public interface IDocumentStore
{
    /// <summary>Validates and stores the document and returns its key.</summary>
    /// <param name="content">The document; <see cref="ContentReference.FileName"/> only supplies the extension.</param>
    /// <exception cref="InvalidContentException">Validation refused the content; nothing was stored.</exception>
    Task<string> StoreAsync(ContentReference content);

    /// <summary>Reads a stored document, or null when the key is unknown.</summary>
    /// <param name="key">A key returned by <see cref="StoreAsync"/>.</param>
    Task<ContentReference?> ReadAsync(string key);
}
