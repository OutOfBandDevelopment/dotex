using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace OoBDev.Docs.Tests;

public sealed partial class MarkdownDocument
{
    private MarkdownDocument(string path, IReadOnlyList<PlantUmlBlock> blocks, IReadOnlyList<(int Line, string Text)> prose)
    {
        Path = path;
        Blocks = blocks;
        Prose = prose;
    }

    public string Path { get; }
    public IReadOnlyList<PlantUmlBlock> Blocks { get; }
    public IReadOnlyList<(int Line, string Text)> Prose { get; }

    public static MarkdownDocument Parse(string path)
    {
        var blocks = new List<PlantUmlBlock>();
        var prose = new List<(int, string)>();
        string? fence = null;
        var language = "";
        var start = 0;
        var body = new List<string>();
        var number = 0;
        foreach (var line in File.ReadAllText(path).Split('\n'))
        {
            number++;
            var text = line.TrimEnd('\r');
            if (fence is null)
            {
                var match = FenceRegex().Match(text);
                if (match.Success)
                {
                    fence = match.Groups[1].Value;
                    language = match.Groups[2].Value;
                    start = number;
                    body.Clear();
                    continue;
                }
                prose.Add((number, text));
            }
            else if (text.Trim() == fence)
            {
                if (language == "plantuml")
                {
                    blocks.Add(new PlantUmlBlock(start, string.Join('\n', body)));
                }
                fence = null;
            }
            else
            {
                body.Add(text);
            }
        }
        return new MarkdownDocument(path, blocks, prose);
    }

    public static IEnumerable<string> Find(string root, string relative)
    {
        var full = global::System.IO.Path.Combine(root, relative);
        if (File.Exists(full))
        {
            yield return full;
            yield break;
        }
        if (!Directory.Exists(full))
        {
            yield break;
        }
        foreach (var file in Directory.EnumerateFiles(full, "*.md", SearchOption.AllDirectories))
        {
            yield return file;
        }
    }

    [GeneratedRegex("^(`{3,})(\\w*)\\s*$")]
    private static partial Regex FenceRegex();
}
