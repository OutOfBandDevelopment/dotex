using System;
using System.Text;
using System.Text.RegularExpressions;

namespace OoBDev.Documents.Hpgl;

/// <summary>
/// The PCL <see cref="IVectorSceneParser"/>. PCL has no vector commands of its own; line work arrives as HP-GL/2
/// between the enter (<c>ESC % 0 B</c> or <c>ESC % 1 B</c>) and leave (<c>ESC % 0 A</c>) escapes, so this parser joins those
/// segments and hands them to the HP-GL interpreter. Text, raster graphics and the rest of PCL are not drawn. Never throws on malformed input.
/// </summary>
public sealed partial class PclSceneParser : IVectorSceneParser
{
    /// <inheritdoc />
    public VectorScene Parse(ReadOnlySpan<byte> data)
    {
        var text = Encoding.Latin1.GetString(data);
        var hpgl = new StringBuilder();
        var inside = false;
        var last = 0;
        foreach (var escape in LanguageSwitch().EnumerateMatches(text))
        {
            if (inside)
            {
                hpgl.Append(text, last, escape.Index - last);
            }

            inside = text[escape.Index + escape.Length - 1] == 'B';
            last = escape.Index + escape.Length;
        }

        if (inside)
        {
            hpgl.Append(text, last, text.Length - last);
        }

        return hpgl.Length == 0 ? VectorScene.Empty : HpglToSvgConverter.ParseScene(Encoding.ASCII.GetBytes(hpgl.ToString()));
    }

    [GeneratedRegex("\u001b%-?[0-9]+[ABX]", RegexOptions.CultureInvariant)]
    private static partial Regex LanguageSwitch();
}
