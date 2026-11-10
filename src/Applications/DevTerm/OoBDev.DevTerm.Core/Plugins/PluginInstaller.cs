using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using OoBDev.System.IO;

namespace OoBDev.DevTerm;

/// <summary>
/// Installs a plugin shared as a <c>.zip</c> with <c>plugin.json</c> at its root (decided 2026-10-13). The archive is unpacked to a
/// staging folder, checked, and shown to the user for approval (the same <see cref="PluginApprovalRequest"/> a start-up approval uses)
/// before anything lands in the plugins directory. Nothing is installed on a refusal, a bad archive or a contract mismatch.
/// </summary>
public static class PluginInstaller
{
    /// <summary>The most a plugin archive may unpack to; a larger one is refused (guards against a zip bomb).</summary>
    public const long MaxUnpackedBytes = 256L * 1024 * 1024;

    /// <summary>Installs <paramref name="zipPath"/> into <paramref name="pluginsDirectory"/> after <paramref name="approver"/> agrees; an existing plugin of the same name is replaced.</summary>
    public static PluginInstallResult Install(string zipPath, string pluginsDirectory, IPluginApprover approver)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(zipPath);
        ArgumentException.ThrowIfNullOrWhiteSpace(pluginsDirectory);
        ArgumentNullException.ThrowIfNull(approver);
        if (!File.Exists(zipPath))
        {
            return new(false, $"'{zipPath}' doesn't exist.");
        }

        using var staging = new TempDirectoryHandle();
        var stagingRoot = staging.DirectoryPath;
        try
        {
            if (Unpack(zipPath, stagingRoot) is { } unpackError)
            {
                return new(false, unpackError);
            }
        }
        catch (Exception ex) when (ex is InvalidDataException or IOException or UnauthorizedAccessException)
        {
            return new(false, $"can't read the archive: {ex.Message}");
        }

        var manifest = PluginManifest.TryRead(Path.Combine(stagingRoot, PluginManifest.FileName), out var error);
        if (manifest is null)
        {
            return new(false, $"the archive needs {PluginManifest.FileName} at its root: {error}");
        }

        if (manifest.Contract != PluginLoader.ContractVersion)
        {
            return new(false, $"built for plugin contract {manifest.Contract}; this build implements {PluginLoader.ContractVersion}.");
        }

        var invalid = Path.GetInvalidFileNameChars();
        var folderName = new string([.. manifest.Name.Trim().Select(c => invalid.Contains(c) ? '_' : c)]);
        if (folderName.Trim('.').Length == 0)
        {
            return new(false, $"'{manifest.Name}' can't be used as a folder name.");
        }

        var target = Path.Combine(Path.GetFullPath(pluginsDirectory), folderName);
        var what = manifest.Process is { } process
            ? string.Join(' ', new[] { process.Command }.Concat(process.Arguments)).Replace("{folder}", target, StringComparison.Ordinal)
            : $"load {manifest.Assembly} inside the app";
        var request = new PluginApprovalRequest(manifest.Name, manifest.Version, target, what, PluginHash.Compute(stagingRoot));
        var choice = approver.Ask(request);
        if (choice == PluginApprovalChoice.Deny)
        {
            return new(false, $"not installed: you declined {manifest.Name} {manifest.Version}.");
        }

        Directory.CreateDirectory(pluginsDirectory);
        if (Directory.Exists(target))
        {
            Directory.Delete(target, recursive: true);
        }

        CopyDirectory(stagingRoot, target);
        if (choice == PluginApprovalChoice.Always)
        {
            PluginTrust.Remember(manifest.Name, request.Hash);
        }

        return new(true, $"installed {manifest.Name} {manifest.Version} into {target}.", target);
    }

    private static string? Unpack(string zipPath, string destination)
    {
        var root = Path.GetFullPath(destination) + Path.DirectorySeparatorChar;
        using var archive = ZipFile.OpenRead(zipPath);
        if (archive.Entries.Sum(e => e.Length) > MaxUnpackedBytes)
        {
            return $"the archive unpacks to more than {MaxUnpackedBytes / (1024 * 1024)} MB.";
        }

        foreach (var entry in archive.Entries)
        {
            var path = Path.GetFullPath(Path.Combine(destination, entry.FullName));
            if (!path.StartsWith(root, StringComparison.OrdinalIgnoreCase))
            {
                return $"'{entry.FullName}' points outside the plugin folder.";
            }

            if (entry.FullName.EndsWith('/') || entry.FullName.EndsWith('\\'))
            {
                Directory.CreateDirectory(path);
                continue;
            }

            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            entry.ExtractToFile(path, overwrite: true);
        }

        return null;
    }

    private static void CopyDirectory(string source, string target)
    {
        foreach (var directory in Directory.GetDirectories(source, "*", SearchOption.AllDirectories))
        {
            Directory.CreateDirectory(Path.Combine(target, Path.GetRelativePath(source, directory)));
        }

        Directory.CreateDirectory(target);
        foreach (var file in Directory.GetFiles(source, "*", SearchOption.AllDirectories))
        {
            File.Copy(file, Path.Combine(target, Path.GetRelativePath(source, file)), overwrite: true);
        }
    }
}
