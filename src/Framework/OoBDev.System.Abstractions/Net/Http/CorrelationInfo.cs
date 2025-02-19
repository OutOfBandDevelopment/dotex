namespace OoBDev.System.Net.Http;

/// <summary>
/// Represents information related to correlation and tracking in a system.
/// </summary>
public class CorrelationInfo
{
    /// <summary>
    /// Gets or sets the correlation identifier.
    /// </summary>
    /// <remarks>
    /// The correlation identifier is used to associate related operations or events across different components or services.
    /// </remarks>
    public string? CorrelationId { get; set; }

    /// <summary>
    /// Gets or sets the request identifier.
    /// </summary>
    /// <remarks>
    /// The request identifier is used to uniquely identify a specific request or operation within a system.
    /// </remarks>
    public string? RequestId { get; set; }
}
