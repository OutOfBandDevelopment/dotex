using System.Collections.Generic;

namespace OoBDev.Documents.Ocr;

/// <summary>The Markdown of a document together with the regions it was read from.</summary>
/// <param name="Markdown">Markdown with <see cref="IDocumentOcr.PageMark"/> between pages.</param>
/// <param name="Regions">The text blocks with their page positions, in reading order.</param>
public sealed record OcrLayout(string Markdown, IReadOnlyList<OcrRegion> Regions);
