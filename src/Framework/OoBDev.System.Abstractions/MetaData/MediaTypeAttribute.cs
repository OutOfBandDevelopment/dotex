using System;

namespace OoBDev.System.MetaData;

/// <summary>
/// Attribute used to associate a class with one or more MIME media types.
/// </summary>
/// <param name="mediaType">The media type (e.g., "application/json", "text/html").</param>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class MediaTypeAttribute(string mediaType) : Attribute
{
    /// <summary>
    /// Gets the media type associated with the decorated class.
    /// </summary>
    public string MediaType { get; } = mediaType;
}
