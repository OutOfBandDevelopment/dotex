namespace OoBDev.Documents.Store;

/// <summary>Makes the blob path a stored document gets.</summary>
public interface IDocumentKeyGenerator
{
    /// <summary>Creates a new, unique key that keeps the extension of the original file name.</summary>
    /// <param name="fileName">The name the caller gave the document.</param>
    string Create(string fileName);
}
