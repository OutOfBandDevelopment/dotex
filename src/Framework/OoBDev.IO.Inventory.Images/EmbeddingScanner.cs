using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.AI;

namespace OoBDev.IO.Inventory.Images;

/// <summary>Embeds every image under a folder with any image embedding generator and groups the similar ones; read-only.</summary>
public sealed class EmbeddingScanner
{
    private static readonly Dictionary<string, string> _types = new(StringComparer.OrdinalIgnoreCase)
    {
        [".png"] = "image/png", [".jpg"] = "image/jpeg", [".jpeg"] = "image/jpeg", [".gif"] = "image/gif", [".bmp"] = "image/bmp", [".webp"] = "image/webp",
    };

    private readonly IEmbeddingGenerator<DataContent, Embedding<float>> _generator;

    /// <summary>Creates the scanner.</summary>
    /// <param name="generator">The embedder, for example the ONNX image embedding generator.</param>
    public EmbeddingScanner(IEmbeddingGenerator<DataContent, Embedding<float>> generator)
    {
        ArgumentNullException.ThrowIfNull(generator);
        _generator = generator;
    }

    /// <summary>Embeds the images under a folder and groups those at or above the similarity.</summary>
    /// <param name="root">The folder.</param>
    /// <param name="minSimilarity">Smallest cosine similarity that counts as a match.</param>
    /// <param name="cancellationToken">Cancellation.</param>
    /// <returns>The groups and the skipped files with the reason.</returns>
    public async Task<(IReadOnlyList<IReadOnlyList<string>> Groups, IReadOnlyList<(string Path, string Reason)> Skipped)> ScanAsync(string root, float minSimilarity, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrEmpty(root);
        var vectors = new List<(string, float[])>();
        var skipped = new List<(string, string)>();
        var options = new EnumerationOptions { RecurseSubdirectories = true, IgnoreInaccessible = true, AttributesToSkip = FileAttributes.ReparsePoint };
        foreach (var path in Directory.EnumerateFiles(root, "*", options).Where(p => _types.ContainsKey(Path.GetExtension(p))))
        {
            cancellationToken.ThrowIfCancellationRequested();
            try
            {
                var bytes = await File.ReadAllBytesAsync(path, cancellationToken).ConfigureAwait(false);
                var result = await _generator.GenerateAsync([new DataContent(bytes, _types[Path.GetExtension(path)])], null, cancellationToken).ConfigureAwait(false);
                vectors.Add((path, result[0].Vector.ToArray()));
            }
            catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or InvalidDataException or ArgumentException)
            {
                skipped.Add((path, ex.Message));
            }
        }

        return (new EmbeddingMatcher().Group(vectors, minSimilarity), skipped);
    }
}
