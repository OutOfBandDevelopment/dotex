using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace OoBDev.LlmCodeGen.Cli;

/// <summary>
/// Parses a model response into the files it contains.
/// </summary>
public static partial class ResponseFileExtractor
{
    private static readonly char[] _invalidNameCharacters = [.. Path.GetInvalidPathChars(), ':', '*', '?', '"', '<', '>', '|'];

    [GeneratedRegex(@"^\s*(?:#+\s*)?(?:file(?:name)?\s*:\s*)?(?:\*\*(?<n>[^*\r\n]+?)\*\*|`(?<n>[^`\r\n]+)`)\s*:?\s*$", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture, 1000)]
    private static partial Regex NameLine();

    [GeneratedRegex(@"^\s*//\s*(?<n>\S+\.[A-Za-z0-9]+)\s*$", RegexOptions.ExplicitCapture, 1000)]
    private static partial Regex CommentName();

    [GeneratedRegex(@"^\s*(?<m>`{3,}|~{3,})", RegexOptions.ExplicitCapture, 1000)]
    private static partial Regex Fence();

    /// <summary>
    /// Extracts the files from a response.
    /// </summary>
    /// <param name="response">The model response text.</param>
    /// <returns>The files in the order they appear; empty when the response has no fenced blocks.</returns>
    public static IReadOnlyList<ExtractedFile> Extract(string response)
    {
        var result = new List<ExtractedFile>();
        if (string.IsNullOrEmpty(response))
        {
            return result;
        }

        var lines = response.Replace("\r\n", "\n", StringComparison.Ordinal).Split('\n');
        string? pendingName = null;
        var unknown = 0;
        var error = 0;

        for (var i = 0; i < lines.Length; i++)
        {
            var fence = Fence().Match(lines[i]);
            if (!fence.Success)
            {
                var named = NameLine().Match(lines[i]);
                if (named.Success)
                {
                    pendingName = named.Groups["n"].Value.Trim();
                }
                else if (!string.IsNullOrWhiteSpace(lines[i]))
                {
                    pendingName = null;
                }
                continue;
            }

            var marker = fence.Groups["m"].Value;
            var body = new List<string>();
            var j = i + 1;
            while (j < lines.Length && !lines[j].TrimStart().StartsWith(marker, StringComparison.Ordinal))
            {
                body.Add(lines[j]);
                j++;
            }
            i = j;

            var name = pendingName;
            pendingName = null;
            if (name == null && body.Count > 0)
            {
                var comment = CommentName().Match(body[0]);
                if (comment.Success)
                {
                    name = comment.Groups["n"].Value;
                    body.RemoveAt(0);
                }
            }

            var content = string.Join("\n", body);
            if (name == null)
            {
                unknown++;
                result.Add(new ExtractedFile(string.Create(CultureInfo.InvariantCulture, $"unknown{unknown}.txt"), content));
            }
            else if (TryNormalize(name, out var path))
            {
                result.Add(new ExtractedFile(path, content));
            }
            else
            {
                error++;
                result.Add(new ExtractedFile(
                    string.Create(CultureInfo.InvariantCulture, $"error{error}.txt"),
                    $"// rejected file name: {name}\n{content}"));
            }
        }

        return result;
    }

    private static bool TryNormalize(string name, out string path)
    {
        path = string.Empty;
        var candidate = name.Trim().Replace('\\', '/');
        if (candidate.Length == 0
            || candidate.StartsWith('/')
            || Path.IsPathRooted(candidate)
            || candidate.IndexOfAny(_invalidNameCharacters) >= 0)
        {
            return false;
        }

        var segments = candidate.Split('/');
        if (segments.Any(s => s.Length == 0 || s == "." || s == ".."))
        {
            return false;
        }

        path = string.Join('/', segments);
        return true;
    }
}
