namespace OoBDev.Documents.Markdown;

/// <summary>The outcome of converting one file to Markdown.</summary>
public enum ConversionStatus
{
    /// <summary>The Markdown was written.</summary>
    Converted,

    /// <summary>The Markdown already existed; nothing was done.</summary>
    Unchanged,

    /// <summary>A PDF without enough embedded text; it needs OCR.</summary>
    NeedsOcr,

    /// <summary>A file named .pdf that has no PDF header.</summary>
    NotPdf,

    /// <summary>The file holds no text.</summary>
    Empty,
}
