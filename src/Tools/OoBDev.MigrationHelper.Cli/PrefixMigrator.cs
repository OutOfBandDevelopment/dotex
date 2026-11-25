namespace OoBDev.MigrationHelper.Cli;

/// <summary>Swaps a name prefix in the folder names, file names and text of a code tree.</summary>
public static class PrefixMigrator
{
    /// <summary>Runs the migration.</summary>
    /// <param name="path">The folder to rewrite.</param>
    /// <param name="sourcePrefix">The prefix to replace (case-sensitive).</param>
    /// <param name="targetPrefix">The new prefix.</param>
    /// <param name="dryRun">When true nothing is changed; the output lists what would be.</param>
    /// <param name="output">Receives one line per renamed name, rewritten file or skipped file.</param>
    /// <param name="error">Receives read and write failures; processing continues.</param>
    /// <returns>The counts of changed (or, in a dry run, changeable) items.</returns>
    public static MigrationSummary Run(string path, string sourcePrefix, string targetPrefix, bool dryRun, TextWriter output, TextWriter error)
    {
        var folders = 0;
        var files = 0;
        var rewritten = 0;
        var skipped = 0;
        var verb = dryRun ? "Would rename" : "Rename";

        // Deepest first, so renaming a parent does not invalidate the paths still to be visited.
        foreach (var folder in Directory.EnumerateDirectories(path, "*.*", SearchOption.AllDirectories).OrderByDescending(f => f.Length).ToList())
        {
            var current = Path.GetFileName(folder);
            if (!current.StartsWith(sourcePrefix, StringComparison.Ordinal)) continue;

            var next = Path.Combine(Path.GetDirectoryName(folder) ?? ".", current.Replace(sourcePrefix, targetPrefix, StringComparison.Ordinal));
            output.WriteLine($"{verb}: {current}");
            if (!dryRun) Directory.Move(folder, next);
            folders++;
        }

        foreach (var file in Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories).ToList())
        {
            var current = Path.GetFileName(file);
            if (!current.StartsWith(sourcePrefix, StringComparison.Ordinal)) continue;

            var next = Path.Combine(Path.GetDirectoryName(file) ?? ".", current.Replace(sourcePrefix, targetPrefix, StringComparison.Ordinal));
            output.WriteLine($"{verb}: {current}");
            if (!dryRun) File.Move(file, next);
            files++;
        }

        // After the renames on a real run; in a dry run nothing moved, so the original paths still exist.
        foreach (var file in Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories).ToList())
        {
            try
            {
                var content = File.ReadAllText(file);
                if (!content.Contains(sourcePrefix, StringComparison.Ordinal)) continue;

                if (content.Contains('\0'))
                {
                    output.WriteLine($"Skip: {file}");
                    skipped++;
                    continue;
                }

                output.WriteLine($"{(dryRun ? "Would rewrite" : "Rewrite")}: {file}");
                if (!dryRun) File.WriteAllText(file, content.Replace(sourcePrefix, targetPrefix, StringComparison.Ordinal));
                rewritten++;
            }
            catch (Exception ex)
            {
                error.WriteLine(ex.Message);
            }
        }

        return new MigrationSummary(folders, files, rewritten, skipped);
    }
}
