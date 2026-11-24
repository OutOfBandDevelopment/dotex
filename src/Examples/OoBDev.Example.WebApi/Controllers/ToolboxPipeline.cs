using OoBDev.System.Codecs;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace OoBDev.WebApi.Controllers;

/// <summary>
/// The byte transforms the Toolbox pipeline builder chains together.
/// </summary>
public static class ToolboxPipeline
{
    private const int MaxBytes = 1_000_000;

    private static readonly Dictionary<string, Func<byte[], byte[]>> Steps = new(StringComparer.Ordinal)
    {
        ["hex"] = b => global::System.Text.Encoding.ASCII.GetBytes(Convert.ToHexString(b)),
        ["hex-decode"] = b => Convert.FromHexString(global::System.Text.Encoding.UTF8.GetString(b).Trim()),
        ["base64"] = b => global::System.Text.Encoding.ASCII.GetBytes(Convert.ToBase64String(b)),
        ["base64-decode"] = b => Convert.FromBase64String(global::System.Text.Encoding.UTF8.GetString(b).Trim()),
        ["base32"] = b => global::System.Text.Encoding.ASCII.GetBytes(new Base32Codec().Encode(b)),
        ["base32-decode"] = b => new Base32Codec().Decode(global::System.Text.Encoding.UTF8.GetString(b).Trim()),
        ["gzip"] = Gzip,
        ["gunzip"] = Gunzip,
        ["sha256"] = global::System.Security.Cryptography.SHA256.HashData,
        ["sha512"] = global::System.Security.Cryptography.SHA512.HashData,
        ["reverse"] = b => [.. b.Reverse()],
    };

    /// <summary>Gets the step keys in display order.</summary>
    public static IReadOnlyList<string> Keys { get; } = [.. Steps.Keys];

    /// <summary>Runs the steps over the input and returns the bytes after each step.</summary>
    /// <param name="input">The starting bytes.</param>
    /// <param name="steps">Step keys, in order.</param>
    /// <returns>One byte array per step.</returns>
    /// <exception cref="ArgumentException">A step key is unknown or the data grows past one million bytes.</exception>
    public static IReadOnlyList<byte[]> Run(byte[] input, IEnumerable<string> steps)
    {
        var results = new List<byte[]>();
        var current = input;
        foreach (var key in steps)
        {
            if (!Steps.TryGetValue(key, out var step))
                throw new ArgumentException($"Unknown step '{key}'. Use one of: {string.Join(", ", Keys)}.", nameof(steps));
            current = step(current);
            if (current.Length > MaxBytes)
                throw new ArgumentException("The data grew past 1000000 bytes.", nameof(steps));
            results.Add(current);
        }

        return results;
    }

    private static byte[] Gzip(byte[] data)
    {
        using var output = new MemoryStream();
        using (var gz = new GZipStream(output, CompressionLevel.Optimal, leaveOpen: true))
            gz.Write(data);
        return output.ToArray();
    }

    private static byte[] Gunzip(byte[] data)
    {
        using var input = new MemoryStream(data);
        using var gz = new GZipStream(input, CompressionMode.Decompress);
        using var output = new MemoryStream();
        var buffer = new byte[8192];
        int read;
        while ((read = gz.Read(buffer)) > 0)
        {
            output.Write(buffer, 0, read);
            if (output.Length > MaxBytes)
                throw new ArgumentException("The data grew past 1000000 bytes.", nameof(data));
        }

        return output.ToArray();
    }
}
