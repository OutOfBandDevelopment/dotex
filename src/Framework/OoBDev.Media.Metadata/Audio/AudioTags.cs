using System.Collections.Generic;

namespace OoBDev.Media.Metadata.Audio;

/// <summary>
/// The tags found in an audio file, under lower-case keys that do not depend on the tag format.
/// </summary>
/// <param name="Format">The tag format that was read: <c>id3v2</c>, <c>id3v1</c>, <c>vorbis</c> (FLAC), or empty when there is none.</param>
/// <param name="Values">The tags by key: <c>title</c>, <c>artist</c>, <c>album</c>, <c>albumartist</c>, <c>track</c>, <c>disc</c>, <c>date</c>, <c>genre</c>, <c>comment</c>. Missing tags are absent.</param>
public sealed record AudioTags(string Format, IReadOnlyDictionary<string, string> Values)
{
    /// <summary>A result with no tags.</summary>
    public static AudioTags Empty { get; } = new(string.Empty, new Dictionary<string, string>());
}
