namespace OoBDev.Communications.Models;

/// <summary>
/// Represents a model for referencing attachments.
/// </summary>
public class AttachmentReferenceModel
{
    /// <summary>
    /// Gets the name of the container where the attachment is stored.
    /// </summary>
    public required string ContainerName { get; init; }

    /// <summary>
    /// Gets the key or identifier of the document associated with the attachment.
    /// </summary>
    public required string DocumentKey { get; init; }
}
