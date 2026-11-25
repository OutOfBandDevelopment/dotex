using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using OoBDev.Onnx.ImageEmbeddings;
using OoBDev.Onnx.ImageEmbeddings.Skia;
using OoBDev.Vision.Dinov2Small;
using OoBDev.IO.Inventory;
using OoBDev.IO.Inventory.Images;

namespace OoBDev.FileInventory.Cli;

/// <summary>Runs the file-inventory commands against an in-memory store, so each call is one scan and report.</summary>
public static class InventoryCommand
{
    /// <summary>The command line help.</summary>
    public const string Usage = """
        file-inventory scan --root <folder> [--follow-links true]
        Hashes every file under the folder (a fast hash first), confirms equal-size equal-hash files with SHA-512 and prints the duplicate groups.
        file-inventory images --root <folder> [--max-distance 5]
        Hashes every image (dHash) and prints groups of near-duplicate images.
        file-inventory images --root <folder> --embeddings dinov2 [--min-similarity 0.95]
        Matches images by DINOv2-small embeddings (the model downloads on first use) instead of the dHash.
        Exit code 0 nothing found, 1 duplicates found, 2 error.
        """;

    /// <summary>Runs one command line.</summary>
    /// <param name="args">The arguments, command first.</param>
    /// <param name="output">Where results go.</param>
    /// <param name="error">Where errors go.</param>
    /// <param name="cancellationToken">Cancellation.</param>
    /// <returns>The exit code.</returns>
    public static async Task<int> RunAsync(string[] args, TextWriter output, TextWriter error, CancellationToken cancellationToken, Func<CancellationToken, Task<IEmbeddingGenerator<DataContent, Embedding<float>>>>? embedderFactory = null)
    {
        ArgumentNullException.ThrowIfNull(args);
        if (args.Length == 0 || args[0] is not ("scan" or "images"))
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

        if (args[0] == "images")
        {
            IReadOnlyList<IReadOnlyList<string>> nearGroups;
            IReadOnlyList<(string Path, string Reason)> skipped;
            if (config["embeddings"] is { Length: > 0 } kind)
            {
                if (!string.Equals(kind, "dinov2", StringComparison.OrdinalIgnoreCase))
                {
                    await error.WriteLineAsync("--embeddings supports: dinov2.").ConfigureAwait(false);
                    return 2;
                }

                var embedder = await (embedderFactory ?? CreateDinov2Async)(cancellationToken).ConfigureAwait(false);
                using var disposable = embedder as IDisposable;
                (nearGroups, skipped) = await new EmbeddingScanner(embedder).ScanAsync(root, float.TryParse(config["min-similarity"], global::System.Globalization.NumberStyles.Float, global::System.Globalization.CultureInfo.InvariantCulture, out var similarity) ? similarity : 0.95f, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                (nearGroups, skipped) = new NearDuplicateFinder().Scan(root, int.TryParse(config["max-distance"], out var distance) ? distance : 5, cancellationToken);
            }

            foreach (var (path, reason) in skipped) await error.WriteLineAsync($"skipped {path}: {reason}").ConfigureAwait(false);
            for (var i = 0; i < nearGroups.Count; i++)
            {
                await output.WriteLineAsync($"near-duplicate group {i + 1}: {nearGroups[i].Count} images").ConfigureAwait(false);
                foreach (var path in nearGroups[i]) await output.WriteLineAsync($"  {path}").ConfigureAwait(false);
            }

            await output.WriteLineAsync($"{nearGroups.Count} near-duplicate group(s), skipped {skipped.Count}").ConfigureAwait(false);
            return nearGroups.Count == 0 ? 0 : 1;
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

    private static async Task<IEmbeddingGenerator<DataContent, Embedding<float>>> CreateDinov2Async(CancellationToken cancellationToken)
    {
        var options = new OnnxImageEmbeddingOptions();
        Dinov2SmallModel.ApplyDefaults(options);
        await Dinov2SmallModel.EnsureAsync(options, NullLogger.Instance, cancellationToken).ConfigureAwait(false);
        return new OnnxImageEmbeddingGenerator(Options.Create(options), new SkiaImageDecoder(), NullLogger<OnnxImageEmbeddingGenerator>.Instance);
    }
}
