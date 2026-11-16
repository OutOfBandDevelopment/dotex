namespace OoBDev.Documents.Markdown;

/// <summary>The result of <see cref="DocumentMarkdownConverter.Convert"/>.</summary>
/// <param name="Status">What happened.</param>
/// <param name="Detail">The Markdown path for converted and unchanged files, otherwise the reason.</param>
public sealed record ConversionResult(ConversionStatus Status, string Detail);
