namespace OoBDev.Media.Metadata.Dates;

/// <summary>
/// A place or event hint left in the path component that carried the date.
/// </summary>
/// <param name="Text">The words.</param>
/// <param name="Confidence">The confidence, 0.30.</param>
/// <param name="Detail">A sentence saying where the words were found.</param>
public sealed record PathPlace(string Text, double Confidence, string Detail);
