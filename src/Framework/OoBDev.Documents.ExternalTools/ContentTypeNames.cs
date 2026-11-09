using System;
using System.Collections.Generic;
using System.Linq;

namespace OoBDev.Documents.ExternalTools;

/// <summary>Maps the short format names and file extensions used in tool registrations to content types.</summary>
public static class ContentTypeNames
{
    private static readonly Dictionary<string, string[]> _map = new(StringComparer.OrdinalIgnoreCase)
    {
        ["hpgl"] = ["image/vnd.hp-hpgl"],
        ["plt"] = ["image/vnd.hp-hpgl"],
        ["ps"] = ["application/postscript"],
        ["eps"] = ["application/postscript"],
        ["postscript"] = ["application/postscript"],
        ["pcl"] = ["application/vnd.hp-pcl"],
        ["pdf"] = ["application/pdf"],
        ["png"] = ["image/png"],
        ["jpg"] = ["image/jpeg"],
        ["jpeg"] = ["image/jpeg"],
        ["bmp"] = ["image/bmp"],
        ["gif"] = ["image/gif"],
        ["tif"] = ["image/tiff"],
        ["tiff"] = ["image/tiff"],
        ["svg"] = ["image/svg+xml"],
        ["image"] = ["image/png", "image/jpeg", "image/bmp", "image/gif", "image/tiff"],
    };

    /// <summary>The content types a comma, semicolon or space separated list of format names or extensions stands for; names it does not know are kept as given (a full content type works too).</summary>
    /// <param name="formats">The list.</param>
    /// <returns>Distinct content types.</returns>
    public static string[] Resolve(string formats)
    {
        ArgumentNullException.ThrowIfNull(formats);
        return formats
            .Split([',', ';', ' '], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .SelectMany(token => _map.TryGetValue(token.TrimStart('.'), out var types) ? types : [token])
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    /// <summary>The file extension (no dot) for a content type, or <c>bin</c>.</summary>
    /// <param name="contentType">The content type.</param>
    /// <returns>The extension.</returns>
    public static string ExtensionFor(string contentType)
    {
        ArgumentNullException.ThrowIfNull(contentType);
        return contentType.ToLowerInvariant() switch
        {
            "image/vnd.hp-hpgl" => "plt",
            "application/postscript" => "ps",
            "application/vnd.hp-pcl" => "pcl",
            "application/pdf" => "pdf",
            "image/png" => "png",
            "image/jpeg" => "jpg",
            "image/bmp" => "bmp",
            "image/gif" => "gif",
            "image/tiff" => "tif",
            "image/svg+xml" => "svg",
            _ => "bin",
        };
    }
}
