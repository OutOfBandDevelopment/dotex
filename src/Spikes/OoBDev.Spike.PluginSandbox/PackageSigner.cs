using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;

namespace OoBDev.Spike.PluginSandbox;

/// <summary>
/// Builds a signed package folder: hashes every file, writes <c>manifest.json</c> and a detached <c>manifest.sig</c> (ECDSA P-256 over the
/// exact manifest bytes, as <c>{ "keyId", "value" }</c>). The signature covers the manifest bytes and the manifest carries a SHA-256 per file.
/// </summary>
public static class PackageSigner
{
    /// <summary>File name of the manifest.</summary>
    public const string ManifestFile = "manifest.json";

    /// <summary>File name of the detached signature.</summary>
    public const string SignatureFile = "manifest.sig";

    /// <summary>Hashes the files under <paramref name="directory"/> into <paramref name="template"/> and signs the result.</summary>
    public static PluginManifest Sign(string directory, PluginManifest template, ECDsa key, string keyId)
    {
        ArgumentNullException.ThrowIfNull(template);
        ArgumentNullException.ThrowIfNull(key);
        var files = new SortedDictionary<string, string>(StringComparer.Ordinal);
        foreach (var relative in PackageFiles(directory))
        {
            files[relative] = Digest(Path.Combine(directory, relative));
        }

        var manifest = template with { Files = files };
        var bytes = JsonSerializer.SerializeToUtf8Bytes(manifest, PluginManifest.Json);
        File.WriteAllBytes(Path.Combine(directory, ManifestFile), bytes);
        var signature = new { keyId, value = Convert.ToBase64String(key.SignData(bytes, HashAlgorithmName.SHA256)) };
        File.WriteAllText(Path.Combine(directory, SignatureFile), JsonSerializer.Serialize(signature, PluginManifest.Json));
        return manifest;
    }

    internal static string Digest(string path)
    {
        using var stream = File.OpenRead(path);
        return "sha256:" + Convert.ToHexString(SHA256.HashData(stream)).ToLowerInvariant();
    }

    internal static IEnumerable<string> PackageFiles(string directory) => Directory
        .EnumerateFiles(directory, "*", SearchOption.AllDirectories)
        .Select(path => Path.GetRelativePath(directory, path).Replace('\\', '/'))
        .Where(relative => relative is not (ManifestFile or SignatureFile));
}
