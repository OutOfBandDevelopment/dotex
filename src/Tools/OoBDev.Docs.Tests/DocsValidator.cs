using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace OoBDev.Docs.Tests;

public static partial class DocsValidator
{
    public static IEnumerable<string> CheckPlantUml(string root, MarkdownDocument document)
    {
        var rel = Path.GetRelativePath(root, document.Path);
        foreach (var block in document.Blocks)
        {
            var text = block.Text.Trim();
            if (!StartRegex().IsMatch(text) || !EndRegex().IsMatch(text))
            {
                yield return $"{rel}:{block.Line}: plantuml block must start with @startuml/@startsalt and end with the matching @end";
                continue;
            }
            if (RemoteIncludeRegex().IsMatch(block.Text))
            {
                yield return $"{rel}:{block.Line}: remote/C4 !include is not allowed (use plain PlantUML)";
            }
            if (!block.Text.Contains('\n') && UnbalancedRegex().IsMatch(block.Text))
            {
                yield return $"{rel}:{block.Line}: unbalanced [ on a line (literal newline inside brackets?)";
            }
        }
    }

    public static IEnumerable<string> CheckLinks(string root, MarkdownDocument document)
    {
        var rel = Path.GetRelativePath(root, document.Path);
        var directory = Path.GetDirectoryName(document.Path)!;
        foreach (var (line, text) in document.Prose)
        {
            foreach (Match match in LinkRegex().Matches(text))
            {
                var target = Path.GetFullPath(Path.Combine(directory, Uri.UnescapeDataString(match.Groups[1].Value)));
                if (!File.Exists(target) && !Directory.Exists(target))
                {
                    yield return $"{rel}:{line}: broken link {match.Groups[1].Value}";
                }
            }
        }
    }

    public static IEnumerable<string> CheckCaptions(string root, MarkdownDocument document)
    {
        var rel = Path.GetRelativePath(root, document.Path);
        foreach (var (kind, pattern) in new[] { ("Figure", FigureRegex()), ("Table", TableRegex()) })
        {
            var numbers = document.Prose
                .Select(p => pattern.Match(p.Text))
                .Where(m => m.Success)
                .Select(m => int.Parse(m.Groups[1].Value, CultureInfo.InvariantCulture))
                .ToList();
            if (!numbers.SequenceEqual(numbers.Order()))
            {
                yield return $"{rel}: {kind} captions out of order [{string.Join(", ", numbers)}]";
            }
        }
    }

    /// <summary>Style guide names: a numbered prefix (<c>01-</c>) only inside <c>docs/patterns-discovery</c>, and project readmes spelled <c>README.{Name}.md</c>.</summary>
    public static IEnumerable<string> CheckNames(string root, MarkdownDocument document)
    {
        var rel = Path.GetRelativePath(root, document.Path).Replace('\\', '/');
        var name = Path.GetFileName(rel);
        if (name.StartsWith("readme.", StringComparison.OrdinalIgnoreCase) && !name.StartsWith("README.", StringComparison.Ordinal)
            && !rel.StartsWith("docs/generated/", StringComparison.Ordinal))
        {
            yield return $"{rel}: project readmes are named README.{{Name}}.md";
        }

        if (rel.Split('/').Any(s => NumberedRegex().IsMatch(s))
            && !rel.StartsWith("docs/patterns-discovery/", StringComparison.Ordinal)
            && !rel.Contains("/images/", StringComparison.Ordinal))
        {
            yield return $"{rel}: numbered names are only used under docs/patterns-discovery";
        }
    }

    [GeneratedRegex(@"^\d{2}-")]
    private static partial Regex NumberedRegex();

    [GeneratedRegex(@"^@start(uml|salt)\b")]
    private static partial Regex StartRegex();

    [GeneratedRegex(@"@end(uml|salt)\s*$")]
    private static partial Regex EndRegex();

    [GeneratedRegex(@"^\s*!include(url|sub)?\s+.*(C4|https?://)", RegexOptions.Multiline)]
    private static partial Regex RemoteIncludeRegex();

    [GeneratedRegex(@"\[[^\]\n]*$", RegexOptions.Multiline)]
    private static partial Regex UnbalancedRegex();

    [GeneratedRegex(@"\]\((?!https?:|mailto:|#)([^)#\s]+)(#[^)\s]*)?\)")]
    private static partial Regex LinkRegex();

    [GeneratedRegex(@"^\*Figure (\d+) — ")]
    private static partial Regex FigureRegex();

    [GeneratedRegex(@"^\*\*Table (\d+) — ")]
    private static partial Regex TableRegex();
}
