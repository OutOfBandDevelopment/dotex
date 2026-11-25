using System;
using System.Collections.Generic;
using System.Formats.Tar;
using System.IO;
using System.IO.Compression;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Data.DesiredState;

/// <summary>
/// Downloads a pinned sqldef release (MIT, github.com/sqldef/sqldef) into a per-user cache, once, and checks the archive against a pinned SHA-256
/// before extracting it. Nothing runs from a download whose hash differs.
/// </summary>
public sealed class SqldefInstaller
{
    /// <summary>The release the pinned hashes belong to.</summary>
    public const string Version = "3.11.30";

    private static readonly Dictionary<string, string> PinnedHashes = new(StringComparer.Ordinal)
    {
        ["psqldef_windows_amd64.zip"] = "744bec47d1cf2d49bb0a36022b36fb826c920e0b485aa225bd47e154aaef6907",
        ["psqldef_linux_amd64.tar.gz"] = "143466d61d8e3fac69c6cb098ccf426df1ae669d28f7edf2acb882a6c298718e",
        ["psqldef_linux_arm64.tar.gz"] = "db8a2da5946d555ae5d8699f9711f40b3842aabc176f897ee5073a682a430361",
        ["psqldef_darwin_amd64.zip"] = "d31112f9d108c37fbbfae57535c40c49668862e3934a9d8be979460c3cfad35b",
        ["psqldef_darwin_arm64.zip"] = "5c3d12eb03097c4e6819db026b385051cafb7934aa82da3819022ca45e4120a1",
        ["sqlite3def_windows_amd64.zip"] = "5dbd44ceb02e1fe79257e2571e3490e30caac04ba54efb98e70dd36f34b228e9",
        ["sqlite3def_linux_amd64.tar.gz"] = "a9666e444df101ce438bb8e263db5c7528371305b0f5ecd2fbb3985bde35d73d",
        ["sqlite3def_linux_arm64.tar.gz"] = "7ec377819554a7eb9093f1aa8ed45a2a4100b5ac09a8e6c63b0f53b2eaa39554",
        ["sqlite3def_darwin_amd64.zip"] = "3ed3f034a5edcf82badd34df33b7fdb14ac89d47169ed74a7049a28c36bc9777",
        ["sqlite3def_darwin_arm64.zip"] = "8352518a5b4925d80bd2dfac9b569145aa6d08c24058b660a219210e3839bd32",
        ["mssqldef_windows_amd64.zip"] = "e20fb8a1943187fd60cd5cf80a0fef9700f7bfa0b140546643df027a5eb36097",
        ["mssqldef_linux_amd64.tar.gz"] = "40fa502a7f8d1ca79140d8d54063de8c413d730d08f3202a776cdc66b6fe0a0b",
        ["mssqldef_linux_arm64.tar.gz"] = "0dc64fd7901aaf7d8095d7f06b7c99618c407193cfca825a49f6bdb532cd0bfd",
        ["mssqldef_darwin_amd64.zip"] = "b81fac0a049b90b132478b982e80d2e77063cc6076dd8ca60e0b07117407307c",
        ["mssqldef_darwin_arm64.zip"] = "90bfae3ff7f76bc9f4c0c9be07635af26759cdd2eafa573f3f30f9b1bffc3e7b",
    };

    private readonly HttpClient http;
    private readonly string cacheRoot;
    private readonly string platform;
    private readonly IReadOnlyDictionary<string, string> hashes;
    private readonly string baseUrl;

    /// <summary>Creates the installer.</summary>
    /// <param name="http">Client used for the download.</param>
    /// <param name="cacheRoot">Cache folder; defaults to the per-user application data folder.</param>
    /// <param name="platform">Release platform such as <c>windows_amd64</c>; defaults to the current machine.</param>
    /// <param name="hashes">Asset name to SHA-256 (hex); defaults to the pinned table.</param>
    /// <param name="baseUrl">Release download URL without a trailing slash; defaults to the GitHub release of <see cref="Version"/>.</param>
    public SqldefInstaller(HttpClient http, string? cacheRoot = null, string? platform = null, IReadOnlyDictionary<string, string>? hashes = null, string? baseUrl = null)
    {
        this.http = http ?? throw new ArgumentNullException(nameof(http));
        this.cacheRoot = cacheRoot ?? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "OoBDev", "sqldef");
        this.platform = platform ?? CurrentPlatform();
        this.hashes = hashes ?? PinnedHashes;
        this.baseUrl = baseUrl ?? $"https://github.com/sqldef/sqldef/releases/download/v{Version}";
    }

    /// <summary>The release platform of this machine, for example <c>windows_amd64</c>.</summary>
    public static string CurrentPlatform()
    {
        var os = OperatingSystem.IsWindows() ? "windows" : OperatingSystem.IsMacOS() ? "darwin" : "linux";
        var arch = RuntimeInformation.OSArchitecture == Architecture.Arm64 ? "arm64" : "amd64";
        return $"{os}_{arch}";
    }

    /// <summary>Returns the path of the tool, downloading and verifying it when it is not cached yet.</summary>
    /// <param name="tool">One of <c>psqldef</c>, <c>sqlite3def</c>, <c>mssqldef</c>.</param>
    /// <param name="cancellationToken">Cancellation.</param>
    /// <returns>The full path of the executable.</returns>
    public async Task<string> EnsureAsync(string tool, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tool);
        var windows = platform.StartsWith("windows", StringComparison.Ordinal);
        var isZip = windows || platform.StartsWith("darwin", StringComparison.Ordinal);
        var asset = $"{tool}_{platform}{(isZip ? ".zip" : ".tar.gz")}";
        if (!hashes.TryGetValue(asset, out var expected))
        {
            throw new NotSupportedException($"No pinned sqldef release asset '{asset}'.");
        }

        var folder = Path.Combine(cacheRoot, Version);
        var executable = Path.Combine(folder, windows ? tool + ".exe" : tool);
        if (File.Exists(executable))
        {
            return executable;
        }

        var bytes = await http.GetByteArrayAsync(new Uri($"{baseUrl}/{asset}"), cancellationToken).ConfigureAwait(false);
        var actual = Convert.ToHexStringLower(SHA256.HashData(bytes));
        if (!string.Equals(actual, expected, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidDataException($"{asset} has SHA-256 {actual}, expected {expected}; nothing was extracted.");
        }

        Directory.CreateDirectory(folder);
        var partial = executable + ".part";
        var archive = new MemoryStream(bytes);
        await using (archive.ConfigureAwait(false))
        {
            var target = File.Create(partial);
            await using (target.ConfigureAwait(false))
            {
                await ExtractAsync(archive, isZip, Path.GetFileName(executable), target, cancellationToken).ConfigureAwait(false);
            }
        }

        if (!OperatingSystem.IsWindows())
        {
            File.SetUnixFileMode(partial, UnixFileMode.UserRead | UnixFileMode.UserWrite | UnixFileMode.UserExecute | UnixFileMode.GroupRead | UnixFileMode.GroupExecute | UnixFileMode.OtherRead | UnixFileMode.OtherExecute);
        }

        File.Move(partial, executable, overwrite: true);
        return executable;
    }

    private static async Task ExtractAsync(Stream archive, bool isZip, string entryName, Stream target, CancellationToken cancellationToken)
    {
        if (isZip)
        {
            using var zip = new ZipArchive(archive, ZipArchiveMode.Read);
            var entry = zip.GetEntry(entryName) ?? throw new InvalidDataException($"The archive has no {entryName}.");
            var source = await entry.OpenAsync(cancellationToken).ConfigureAwait(false);
            await using (source.ConfigureAwait(false))
            {
                await source.CopyToAsync(target, cancellationToken).ConfigureAwait(false);
            }

            return;
        }

        var gzip = new GZipStream(archive, CompressionMode.Decompress);
        await using (gzip.ConfigureAwait(false))
        {
            var tar = new TarReader(gzip);
            await using (tar.ConfigureAwait(false))
            {
                while (await tar.GetNextEntryAsync(cancellationToken: cancellationToken).ConfigureAwait(false) is { } entry)
                {
                    if (entry.EntryType is TarEntryType.RegularFile or TarEntryType.V7RegularFile && Path.GetFileName(entry.Name) == entryName && entry.DataStream is { } data)
                    {
                        await data.CopyToAsync(target, cancellationToken).ConfigureAwait(false);
                        return;
                    }
                }
            }
        }

        throw new InvalidDataException($"The archive has no {entryName}.");
    }
}
