using System;

namespace OoBDev.System.IO.Segmenters;

/// <summary>
/// Exception thrown when a data segmentation operation fails or produces invalid results.
/// </summary>
[Serializable]
public class InvalidSegmentationException : Exception
{
    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidSegmentationException"/> class.
    /// </summary>
    public InvalidSegmentationException()
    {
    }
}