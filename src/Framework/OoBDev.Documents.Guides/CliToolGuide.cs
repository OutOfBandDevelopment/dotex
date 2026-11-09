using System;
using System.Collections.Generic;
using System.Text;

namespace OoBDev.Documents.Guides;

/// <summary>Renders a command-line tool's guide as markdown from captured runs; the same runs always give the same bytes.</summary>
public static class CliToolGuide
{
    /// <summary>Renders the guide.</summary>
    /// <param name="title">Document title.</param>
    /// <param name="synopsis">What the tool is for; may be empty.</param>
    /// <param name="examples">Captured runs, rendered in the given order.</param>
    public static string Render(string title, string synopsis, IReadOnlyList<CliExample> examples)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentNullException.ThrowIfNull(examples);

        var text = new StringBuilder();
        text.Append("# ").Append(title).Append('\n');
        if (!string.IsNullOrWhiteSpace(synopsis))
        {
            text.Append('\n').Append(Normalize(synopsis)).Append('\n');
        }

        foreach (var example in examples)
        {
            text.Append("\n## ").Append(example.Title).Append('\n');
            if (!string.IsNullOrWhiteSpace(example.Description))
            {
                text.Append('\n').Append(Normalize(example.Description)).Append('\n');
            }

            text.Append("\n```console\n$ ").Append(example.CommandLine).Append('\n');
            var output = Normalize(example.Output);
            if (output.Length > 0)
            {
                text.Append(output).Append('\n');
            }

            text.Append("```\n");
            if (example.ExitCode != 0)
            {
                text.Append("\nExit code: `").Append(example.ExitCode).Append("`\n");
            }
        }

        return text.ToString();
    }

    private static string Normalize(string value) =>
        value.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n').TrimEnd();
}
