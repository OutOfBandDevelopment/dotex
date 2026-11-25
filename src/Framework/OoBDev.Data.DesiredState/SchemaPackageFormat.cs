using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text.Json;

namespace OoBDev.Data.DesiredState;

/// <summary>
/// Zip layout: <c>manifest.json</c> (<c>name</c>, <c>version</c>), <c>schema/*.sql</c>, <c>predeploy/*.sql</c>, <c>postdeploy/*.sql</c> (each in file-name order)
/// and <c>refactorlog.json</c> (an array of <c>id</c>, <c>table</c>, <c>sql</c>, in order).
/// </summary>
public static class SchemaPackageFormat
{
    private const string Manifest = "manifest.json";
    private const string RefactorLog = "refactorlog.json";
    private static readonly JsonSerializerOptions _json = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    public static void Write(SchemaPackage package, Stream destination)
    {
        ArgumentNullException.ThrowIfNull(package);
        ArgumentNullException.ThrowIfNull(destination);
        using var zip = new ZipArchive(destination, ZipArchiveMode.Create, leaveOpen: true);
        Add(zip, Manifest, JsonSerializer.Serialize(new ManifestFile(package.Name, package.Version), _json));
        Add(zip, RefactorLog, JsonSerializer.Serialize(package.Refactors, _json));
        foreach (var (folder, schema) in new[] { ("schema", package.Schema), ("predeploy", package.PreDeploy), ("postdeploy", package.PostDeploy) })
        {
            foreach (var script in schema.Scripts)
            {
                Add(zip, $"{folder}/{script.Name}", script.Text);
            }
        }
    }

    public static SchemaPackage Read(Stream source)
    {
        ArgumentNullException.ThrowIfNull(source);
        using var zip = new ZipArchive(source, ZipArchiveMode.Read, leaveOpen: true);
        var manifest = JsonSerializer.Deserialize<ManifestFile>(Text(zip, Manifest) ?? throw new InvalidDataException($"The package has no {Manifest}."), _json)
            ?? throw new InvalidDataException($"{Manifest} is empty.");
        var refactors = Text(zip, RefactorLog) is { } log ? JsonSerializer.Deserialize<List<RefactorEntry>>(log, _json) ?? [] : [];
        return new SchemaPackage(manifest.Name, manifest.Version, Scripts(zip, "schema"), Scripts(zip, "predeploy"), Scripts(zip, "postdeploy"), refactors);
    }

    private static DesiredSchema Scripts(ZipArchive zip, string folder) =>
        new([.. zip.Entries
            .Where(e => e.FullName.StartsWith(folder + "/", StringComparison.Ordinal) && e.Name.Length > 0)
            .OrderBy(e => e.Name, StringComparer.Ordinal)
            .Select(e => new DesiredScript(e.Name, Read(e)))]);

    private static string? Text(ZipArchive zip, string name) => zip.GetEntry(name) is { } entry ? Read(entry) : null;

    private static string Read(ZipArchiveEntry entry)
    {
        using var reader = new StreamReader(entry.Open());
        return reader.ReadToEnd();
    }

    private static void Add(ZipArchive zip, string name, string text)
    {
        var entry = zip.CreateEntry(name);
        using var writer = new StreamWriter(entry.Open());
        writer.Write(text);
    }

    private sealed record ManifestFile(string Name, string Version);
}
