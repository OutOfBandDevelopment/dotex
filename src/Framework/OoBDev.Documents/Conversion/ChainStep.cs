namespace OoBDev.Documents.Conversion;

/// <summary>
/// Represents a step in a document conversion chain.
/// </summary>
public record ChainStep
{
    /// <summary>
    /// Gets or sets the document conversion handler for this step.
    /// </summary>
    public required IDocumentConversionHandler Handler { get; init; }

    /// <summary>
    /// Gets or sets the content type of the source document for this step.
    /// </summary>
    public required string SourceContentType { get; init; }

    /// <summary>
    /// Gets or sets the content type of the destination document for this step.
    /// </summary>
    public required string DestinationContentType { get; init; }
}
