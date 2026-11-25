using System;
using System.Globalization;
using System.Linq;
using System.Text;

namespace OoBDev.UiDefinitions;

/// <summary>Shared formatting for <see cref="ICommandPreview"/> implementations, so every surface renders previews the same way.</summary>
public static class CommandPreviewFormat
{
    /// <summary>Escapes control characters visibly: <c>\r</c>, <c>\n</c>, <c>\t</c>, <c>\\</c>, and <c>\xNN</c> for any other control character.</summary>
    public static string EscapeControlCharacters(string text)
    {
        ArgumentNullException.ThrowIfNull(text);

        var builder = new StringBuilder(text.Length + 4);
        foreach (var c in text)
        {
            switch (c)
            {
                case '\r':
                    builder.Append("\\r");
                    break;
                case '\n':
                    builder.Append("\\n");
                    break;
                case '\t':
                    builder.Append("\\t");
                    break;
                case '\\':
                    builder.Append("\\\\");
                    break;
                default:
                    if (char.IsControl(c))
                    {
                        builder.Append("\\x").Append(((int)c).ToString("X2", CultureInfo.InvariantCulture));
                    }
                    else
                    {
                        builder.Append(c);
                    }

                    break;
            }
        }

        return builder.ToString();
    }

    /// <summary>Space-separated upper-case hex bytes, e.g. <c>00 05 01 FF</c>.</summary>
    public static string ToHex(byte[] bytes)
    {
        ArgumentNullException.ThrowIfNull(bytes);
        return string.Join(' ', bytes.Select(b => b.ToString("X2", CultureInfo.InvariantCulture)));
    }
}
