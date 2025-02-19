namespace OoBDev.Documents.Conversion;

/// <summary>
/// Represents a document conversion chain builder.
/// </summary>
public interface IDocumentConversionChainBuilder
{
    /// <summary>
    /// Gets the conversion steps from a source content type to a destination content type.
    /// </summary>
    /// <param name="sourceContentType">The content type of the source content.</param>
    /// <param name="destinationContentType">The content type of the destination content.</param>
    /// <returns>An array of <see cref="ChainStep"/> representing the conversion steps.</returns>
    ChainStep[] Steps(string sourceContentType, string destinationContentType);
}
