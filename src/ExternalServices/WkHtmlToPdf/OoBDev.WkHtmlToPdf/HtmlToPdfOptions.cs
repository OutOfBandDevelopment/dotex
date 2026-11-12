using WkHtmlToPdfDotNet;

namespace OoBDev.WkHtmlToPdf;

/// <summary>
/// Page settings for <see cref="HtmlToPdfConversionHandler"/>. The defaults reproduce the previously fixed output.
/// </summary>
public class HtmlToPdfOptions
{
    /// <summary>Gets or sets whether the PDF is in colour or grayscale.</summary>
    public ColorMode ColorMode { get; set; } = ColorMode.Color;

    /// <summary>Gets or sets the page orientation.</summary>
    public Orientation Orientation { get; set; } = Orientation.Portrait;

    /// <summary>Gets or sets the paper size.</summary>
    public PaperKind PaperSize { get; set; } = PaperKind.Letter;

    /// <summary>Gets or sets the default text encoding of the HTML.</summary>
    public string DefaultEncoding { get; set; } = "utf-8";

    /// <summary>Gets or sets the right-aligned header text; wkhtmltopdf placeholders such as <c>[page]</c> and <c>[toPage]</c> are replaced. Empty for no header text.</summary>
    public string HeaderRight { get; set; } = "Page [page] of [toPage]";

    /// <summary>Gets or sets the header font size in points.</summary>
    public int HeaderFontSize { get; set; } = 9;

    /// <summary>Gets or sets whether a line is drawn under the header.</summary>
    public bool HeaderLine { get; set; } = true;
}
