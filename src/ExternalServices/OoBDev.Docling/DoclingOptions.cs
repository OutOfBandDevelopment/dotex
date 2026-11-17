using System.ComponentModel.DataAnnotations;

namespace OoBDev.Docling;

/// <summary>Settings of a docling-serve instance.</summary>
public sealed class DoclingOptions
{
    /// <summary>The configuration section the options bind from.</summary>
    public const string SectionName = "Docling";

    /// <summary>The server root, for example <c>http://localhost:5001</c>.</summary>
    [Required]
    public string BaseUrl { get; set; } = "";

    /// <summary>How long one conversion may take in total, in seconds.</summary>
    [Range(1, 86400)]
    public int TimeoutSeconds { get; set; } = 1800;

    /// <summary>The seconds one status poll waits on the server.</summary>
    [Range(0, 120)]
    public int PollWaitSeconds { get; set; } = 20;

    /// <summary>Comma-separated OCR languages sent as <c>ocr_lang</c>; empty uses the server default, which needs every default language installed.</summary>
    public string OcrLanguages { get; set; } = "";
}
