namespace OoBDev.Documents.Ocr;

/// <summary>A block of recognised text and where it sits on its page. Positions are fractions of the page (0 to 1, origin top left), so they survive any render scale.</summary>
/// <param name="Page">The 1-based page number.</param>
/// <param name="Label">The block kind the recogniser reported (for example <c>text</c>, <c>section_header</c>, <c>table</c>).</param>
/// <param name="Text">The recognised text; empty when the block has none.</param>
/// <param name="Left">The left edge.</param>
/// <param name="Top">The top edge.</param>
/// <param name="Right">The right edge.</param>
/// <param name="Bottom">The bottom edge.</param>
public sealed record OcrRegion(int Page, string Label, string Text, double Left, double Top, double Right, double Bottom);
