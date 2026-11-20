using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using OoBDev.IO.Inventory;

namespace OoBDev.FileInventory.Cli;

/// <summary>Runs the file-inventory commands against an in-memory store, so each call is one scan and report.</summary>
public static class InventoryCommand
{
    /// <summary>The command line help.</summary>
    public const string Usage = """
        file-inventory scan --root <folder> [--follow-links true]
        Hashes every file under the folder (a fast hash first), confirms equal-size equal-hash files with SHA-512 and prints the duplicate groups.
        Exit code 0 no duplicates, 1 duplicates found, 2 error.
        """;

    /// <summary>Runs one command line.</summary>
    /// <param name="args">The arguments, command first.</param>
    /// <param name="output">Where results go.</param>
    /// <param name="error">Where errors go.</param>
    /// <param name="cancellationToken">Cancellation.</param>
    /// <returns>The exit code.</returns>
    public static async Task<int> RunAsync(string[] args, TextWriter output, TextWriter error, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(args);
        if (args.Length == 0 || args[0] != "scan")
        {
            await error.WriteLineAsync(Usage).ConfigureAwait(false);
            return 2;
        }

        var config = new ConfigurationBuilder().AddCommandLine(args[1..]).Build();
        var root = config["root"];
        if (string.IsNullOrWhiteSpace(root) || !Directory.Exists(root))
        {
            await error.WriteLineAsync("--root must name an existing folder.").ConfigureAwait(false);
            return 2;
        }

        var store = new InMemoryInventoryStore();
        var report = await new FileScanner(store).ScanAsync(root, bool.TryParse(config["follow-links"], out var follow) && follow, cancellationToken).ConfigureAwait(false);
        await output.WriteLineAsync($"scanned {report.Scanned}, hashed {report.Hashed}, skipped {report.Skipped.Count}").ConfigureAwait(false);
        foreach (var (path, reason) in report.Skipped) await error.WriteLineAsync($"skipped {path}: {reason}").ConfigureAwait(false);

        var groups = 0;
        await foreach (var group in new DuplicateFinder().FindAsync(store, cancellationToken).ConfigureAwait(false))
        {
            groups++;
            await output.WriteLineAsync($"duplicate group {groups}: {group.Length} bytes, {group.Paths.Count} files").ConfigureAwait(false);
            foreach (var path in group.Paths.Order(StringComparer.OrdinalIgnoreCase)) await output.WriteLineAsync($"  {path}").ConfigureAwait(false);
        }

        await output.WriteLineAsync($"{groups} duplicate group(s)").ConfigureAwait(false);
        return groups == 0 ? 0 : 1;
    }
}
