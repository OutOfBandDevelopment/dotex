using System.Collections.Generic;
using System.Text;

namespace OoBDev.System.Sandbox;

/// <summary>Splits a launcher command string into arguments.</summary>
public static class CommandLine
{
    // Splits on spaces; a double-quoted part keeps its spaces and loses the quotes.
    /// <summary>Splits on spaces; a double-quoted part keeps its spaces.</summary>
    public static IReadOnlyList<string> Split(string text)
    {
        var parts = new List<string>();
        var current = new StringBuilder();
        var quoted = false;
        foreach (var c in text)
        {
            if (c == '"')
            {
                quoted = !quoted;
            }
            else if (c == ' ' && !quoted)
            {
                if (current.Length > 0)
                {
                    parts.Add(current.ToString());
                    current.Clear();
                }
            }
            else
            {
                current.Append(c);
            }
        }

        if (current.Length > 0)
        {
            parts.Add(current.ToString());
        }

        return parts;
    }
}
