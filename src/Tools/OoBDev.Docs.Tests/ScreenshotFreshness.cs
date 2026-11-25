using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace OoBDev.Docs.Tests;

/// <summary>
/// Finds guide screenshots that are older than the code they show. An image is matched to its source folders by file name
/// prefix (<c>sources.json</c> beside the images); it is stale when the newest commit in those folders is more than the
/// tolerance newer than the newest commit of the image.
/// </summary>
public static class ScreenshotFreshness
{
    /// <summary>Lists the stale images.</summary>
    /// <param name="imageDates">The last commit date of each image, by file name.</param>
    /// <param name="sourcesByPrefix">The source folders for each file name prefix.</param>
    /// <param name="sourceDate">The newest commit date of a set of folders, or <see langword="null"/> when unknown.</param>
    /// <param name="tolerance">How much newer the code may be before the image counts as stale.</param>
    /// <returns>One line per stale image.</returns>
    public static IEnumerable<string> FindStale(
        IReadOnlyDictionary<string, DateTimeOffset> imageDates,
        IReadOnlyDictionary<string, string[]> sourcesByPrefix,
        Func<string[], DateTimeOffset?> sourceDate,
        TimeSpan tolerance)
    {
        ArgumentNullException.ThrowIfNull(imageDates);
        ArgumentNullException.ThrowIfNull(sourcesByPrefix);
        ArgumentNullException.ThrowIfNull(sourceDate);

        var cache = new Dictionary<string, DateTimeOffset?>(StringComparer.Ordinal);
        foreach (var (name, imageDate) in imageDates.OrderBy(i => i.Key, StringComparer.Ordinal))
        {
            var match = sourcesByPrefix.FirstOrDefault(s => name.StartsWith(s.Key, StringComparison.OrdinalIgnoreCase));
            if (match.Value is null)
            {
                yield return $"{name}: no source folders for this prefix in sources.json";
                continue;
            }

            if (!cache.TryGetValue(match.Key, out var code))
            {
                code = sourceDate(match.Value);
                cache[match.Key] = code;
            }

            if (code is { } codeDate && codeDate - imageDate > tolerance)
            {
                yield return string.Create(CultureInfo.InvariantCulture, $"{name}: image {imageDate:yyyy-MM-dd}, code {codeDate:yyyy-MM-dd} ({(codeDate - imageDate).TotalDays:0} days newer)");
            }
        }
    }

    /// <summary>Reads the newest commit date of the given paths with <c>git log</c>.</summary>
    /// <param name="root">The repository root.</param>
    /// <param name="paths">The paths, relative to the root.</param>
    /// <returns>The commit date, or <see langword="null"/> when git has no commit for them.</returns>
    public static DateTimeOffset? GitDate(string root, params string[] paths)
    {
        var info = new ProcessStartInfo("git") { WorkingDirectory = root, RedirectStandardOutput = true, UseShellExecute = false };
        foreach (var argument in new[] { "log", "-1", "--format=%cI", "--" }.Concat(paths))
        {
            info.ArgumentList.Add(argument);
        }

        using var process = Process.Start(info) ?? throw new InvalidOperationException("git did not start.");
        var output = process.StandardOutput.ReadToEnd().Trim();
        process.WaitForExit();
        return DateTimeOffset.TryParse(output, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date) ? date : null;
    }
}
