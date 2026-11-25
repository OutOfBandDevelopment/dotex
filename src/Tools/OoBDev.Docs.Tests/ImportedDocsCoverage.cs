using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace OoBDev.Docs.Tests;

/// <summary>
/// Finds markdown files under <c>docs/applications/{app}</c> that nobody points to: a file counts as covered when the
/// application readme reaches it through links (a link to a folder covers everything below it), or when a document in
/// <c>docs/migration</c> names it or one of its folders (the transient migration map).
/// </summary>
public static partial class ImportedDocsCoverage
{
    public static IEnumerable<string> FindOrphans(string root, string applicationFolder)
    {
        var applicationPath = Path.GetFullPath(applicationFolder);
        var readme = Path.Combine(applicationPath, "README.md");
        if (!File.Exists(readme))
        {
            yield return $"{Relative(root, applicationPath)}: no README.md to link the application documents from";
            yield break;
        }

        var linkedFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var linkedFolders = new List<string>();
        var queue = new Queue<string>([readme]);
        while (queue.TryDequeue(out var current))
        {
            if (!linkedFiles.Add(current))
            {
                continue;
            }

            foreach (var target in LinkTargets(current))
            {
                if (Directory.Exists(target))
                {
                    linkedFolders.Add(target.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar);
                }
                else if (File.Exists(target) && target.EndsWith(".md", StringComparison.OrdinalIgnoreCase)
                    && target.StartsWith(applicationPath, StringComparison.OrdinalIgnoreCase))
                {
                    queue.Enqueue(target);
                }
            }
        }

        var mapText = MigrationMapText(root);
        foreach (var file in Directory.EnumerateFiles(applicationPath, "*.md", SearchOption.AllDirectories).Order(StringComparer.Ordinal))
        {
            if (linkedFiles.Contains(file) || linkedFolders.Any(f => file.StartsWith(f, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            if (MentionedInMap(root, applicationPath, file, mapText))
            {
                continue;
            }

            yield return $"{Relative(root, file)}: not linked from the application readme and not named in docs/migration";
        }
    }

    private static string Relative(string root, string path) => Path.GetRelativePath(root, path).Replace('\\', '/');

    private static IEnumerable<string> LinkTargets(string file)
    {
        var directory = Path.GetDirectoryName(file)!;
        foreach (var (_, text) in MarkdownDocument.Parse(file).Prose)
        {
            foreach (Match match in LinkRegex().Matches(text))
            {
                yield return Path.GetFullPath(Path.Combine(directory, Uri.UnescapeDataString(match.Groups[1].Value)));
            }
        }
    }

    private static string MigrationMapText(string root)
    {
        var folder = Path.Combine(root, "docs", "migration");
        return Directory.Exists(folder)
            ? string.Join('\n', Directory.EnumerateFiles(folder, "*.md").Select(File.ReadAllText)).Replace('\\', '/')
            : string.Empty;
    }

    private static bool MentionedInMap(string root, string applicationPath, string file, string mapText)
    {
        var applicationRelative = Relative(root, applicationPath);
        var path = Relative(root, file);
        while (path.Length > applicationRelative.Length)
        {
            if (mapText.Contains(path, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            path = path[..path.LastIndexOf('/')];
        }

        return false;
    }

    [GeneratedRegex(@"\]\((?!https?:|mailto:|#)([^)#\s]+)(#[^)\s]*)?\)")]
    private static partial Regex LinkRegex();
}
