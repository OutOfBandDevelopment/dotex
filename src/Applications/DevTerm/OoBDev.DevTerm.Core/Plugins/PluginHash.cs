using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace OoBDev.DevTerm;

/// <summary>A SHA-256 over a plugin folder's every file (relative path and content, in a fixed order), so any change to the plugin changes its hash.</summary>
public static class PluginHash
{
    public static string Compute(string folder)
    {
        using var hash = IncrementalHash.CreateHash(HashAlgorithmName.SHA256);
        var root = Path.GetFullPath(folder);
        foreach (var file in Directory.GetFiles(root, "*", SearchOption.AllDirectories).Order(StringComparer.Ordinal))
        {
            hash.AppendData(Encoding.UTF8.GetBytes(Path.GetRelativePath(root, file).Replace(Path.DirectorySeparatorChar, '/') + "\n"));
            hash.AppendData(File.ReadAllBytes(file));
        }

        return Convert.ToHexString(hash.GetHashAndReset());
    }
}
