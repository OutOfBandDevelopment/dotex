using System.Collections.Generic;

namespace OoBDev.Media.Metadata.Descriptors;

/// <summary>The descriptive fields an image can carry in XMP or IPTC.</summary>
/// <param name="Title">Title or object name.</param>
/// <param name="Creator">Author or byline.</param>
/// <param name="Description">Caption or description.</param>
/// <param name="Keywords">Keywords or subjects.</param>
/// <param name="Rating">Star rating (XMP only), or null.</param>
/// <param name="Created">Creation date text as written (ISO 8601 for XMP, yyyyMMdd for IPTC), or null.</param>
public sealed record ImageDescriptors(string? Title, string? Creator, string? Description, IReadOnlyList<string> Keywords, int? Rating, string? Created)
{
    /// <summary>Nothing found.</summary>
    public static ImageDescriptors Empty { get; } = new(null, null, null, [], null, null);

    /// <summary>Whether any field is set.</summary>
    public bool IsEmpty => Title is null && Creator is null && Description is null && Keywords.Count == 0 && Rating is null && Created is null;
}
