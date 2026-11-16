namespace OoBDev.Documents.Markdown;

/// <summary>An inclusive 1-based page range.</summary>
/// <param name="From">First page.</param>
/// <param name="To">Last page; 0 means the whole file.</param>
public sealed record PageRange(int From, int To);
