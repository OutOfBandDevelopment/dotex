using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text.Json;

namespace OoBDev.System.Sandbox;

/// <summary>Verifies a package folder before install (REQ-006): trusted key, valid signature, every file present, unchanged and listed.</summary>
public sealed class PackageVerifier(TrustList trust)
{
    private readonly TrustList _trust = trust ?? throw new ArgumentNullException(nameof(trust));

    /// <summary>Checks <paramref name="directory"/> and returns the manifest only when everything matches.</summary>
    public PackageVerification Verify(string directory)
    {
        var manifestPath = Path.Combine(directory, PackageSigner.ManifestFile);
        var signaturePath = Path.Combine(directory, PackageSigner.SignatureFile);
        if (!File.Exists(manifestPath) || !File.Exists(signaturePath))
        {
            return Refuse(PackageRefusal.MissingSignature, "manifest.json or manifest.sig");
        }

        var bytes = File.ReadAllBytes(manifestPath);
        PluginManifest? manifest;
        string keyId;
        byte[] value;
        try
        {
            manifest = JsonSerializer.Deserialize<PluginManifest>(bytes, PluginManifest.Json);
            using var document = JsonDocument.Parse(File.ReadAllText(signaturePath));
            keyId = document.RootElement.GetProperty("keyId").GetString() ?? string.Empty;
            value = Convert.FromBase64String(document.RootElement.GetProperty("value").GetString() ?? string.Empty);
        }
        catch (Exception ex) when (ex is JsonException or FormatException or KeyNotFoundException or InvalidOperationException)
        {
            return Refuse(PackageRefusal.BadManifest, ex.GetType().Name);
        }

        if (manifest is null || string.IsNullOrWhiteSpace(manifest.Id) || manifest.Files is null || manifest.Permissions is null)
        {
            return Refuse(PackageRefusal.BadManifest, "id, permissions and files are required");
        }

        using var key = _trust.Open(keyId);
        if (key is null)
        {
            return Refuse(PackageRefusal.UnknownKey, keyId);
        }

        return key.VerifyData(bytes, value, HashAlgorithmName.SHA256) ? CheckFiles(directory, manifest) : Refuse(PackageRefusal.BadSignature, keyId);
    }

    private static PackageVerification CheckFiles(string directory, PluginManifest manifest)
    {
        var root = Path.GetFullPath(directory) + Path.DirectorySeparatorChar;
        foreach (var (relative, digest) in manifest.Files)
        {
            var full = Path.GetFullPath(Path.Combine(directory, relative));
            if (!full.StartsWith(root, StringComparison.OrdinalIgnoreCase))
            {
                return Refuse(PackageRefusal.BadPath, relative);
            }

            if (!File.Exists(full))
            {
                return Refuse(PackageRefusal.FileMissing, relative);
            }

            if (!string.Equals(PackageSigner.Digest(full), digest, StringComparison.OrdinalIgnoreCase))
            {
                return Refuse(PackageRefusal.FileModified, relative);
            }
        }

        var unlisted = PackageSigner.PackageFiles(directory).FirstOrDefault(file => !manifest.Files.ContainsKey(file));
        return unlisted is null ? new PackageVerification(PackageRefusal.None, string.Empty, manifest) : Refuse(PackageRefusal.UnlistedFile, unlisted);
    }

    private static PackageVerification Refuse(PackageRefusal refusal, string detail) => new(refusal, detail, null);
}
