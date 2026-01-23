namespace OoBDev.System.IO.Segmenters;

/// <summary>
/// Represents the status of a data segmentation operation.
/// </summary>
public enum SegmentationStatus
{
    /// <summary>
    /// The segment was successfully extracted and is complete.
    /// </summary>
    Complete,

    /// <summary>
    /// The segment is incomplete and requires more data to be fully extracted.
    /// </summary>
    Incomplete,

    /// <summary>
    /// The segment is invalid and could not be properly extracted.
    /// </summary>
    Invalid,
}