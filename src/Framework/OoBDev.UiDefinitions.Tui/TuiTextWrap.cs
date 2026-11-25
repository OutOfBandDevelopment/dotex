using System;
using System.Collections.Generic;
using System.Text;

namespace OoBDev.UiDefinitions.Tui;

/// <summary>Text helpers shared by the Terminal.Gui renderers.</summary>
public static class TuiTextWrap
{
    /// <summary>Greedy word wrap to <paramref name="width"/> columns, keeping explicit line breaks; a single word longer than the width is split.</summary>
    public static List<string> WordWrap(string text, int width)
    {
        var lines = new List<string>();
        foreach (var paragraph in text.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n'))
        {
            var line = new StringBuilder();
            foreach (var word in paragraph.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            {
                var remaining = word;
                while (remaining.Length > 0)
                {
                    var needed = line.Length == 0 ? remaining.Length : line.Length + 1 + remaining.Length;
                    if (needed <= width)
                    {
                        if (line.Length > 0)
                        {
                            line.Append(' ');
                        }

                        line.Append(remaining);
                        remaining = string.Empty;
                    }
                    else if (line.Length > 0)
                    {
                        lines.Add(line.ToString());
                        line.Clear();
                    }
                    else
                    {
                        lines.Add(remaining[..width]);
                        remaining = remaining[width..];
                    }
                }
            }

            lines.Add(line.ToString());
        }

        return lines;
    }
}