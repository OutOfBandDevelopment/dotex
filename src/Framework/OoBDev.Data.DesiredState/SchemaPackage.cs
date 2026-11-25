using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace OoBDev.Data.DesiredState;

/// <summary>
/// A deployable final state, the equivalent of a dacpac: the schema scripts, scripts to run before and after the schema change, and the refactor log.
/// <see cref="SchemaPackageFormat"/> reads and writes it as a zip.
/// </summary>
public sealed record SchemaPackage(
    string Name,
    string Version,
    DesiredSchema Schema,
    DesiredSchema PreDeploy,
    DesiredSchema PostDeploy,
    IReadOnlyList<RefactorEntry> Refactors)
{

    /// <summary>Reads a folder laid out like the zip: <c>schema/</c>, <c>predeploy/</c>, <c>postdeploy/</c> (each optional, <c>*.sql</c>) and an optional <c>refactorlog.json</c>.</summary>
    public static SchemaPackage FromDirectory(string directory, string name, string version)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(directory);
        var log = Path.Combine(directory, "refactorlog.json");
        var refactors = File.Exists(log)
            ? System.Text.Json.JsonSerializer.Deserialize<List<RefactorEntry>>(File.ReadAllText(log), new System.Text.Json.JsonSerializerOptions(System.Text.Json.JsonSerializerDefaults.Web)) ?? []
            : [];
        return new SchemaPackage(name, version, Scripts(directory, "schema"), Scripts(directory, "predeploy"), Scripts(directory, "postdeploy"), refactors);
    }

    private static DesiredSchema Scripts(string directory, string folder)
    {
        var path = Path.Combine(directory, folder);
        return Directory.Exists(path) ? DesiredSchema.FromDirectory(path) : new DesiredSchema([]);
    }

    /// <summary>SHA-512 (hex) over the name, version, every script and every refactor entry, in order.</summary>
    public string Hash
    {
        get
        {
            var text = new StringBuilder().Append(Name).Append('\n').Append(Version).Append('\n');
            foreach (var (kind, schema) in new[] { ("schema", Schema), ("predeploy", PreDeploy), ("postdeploy", PostDeploy) })
            {
                foreach (var script in schema.Scripts)
                {
                    text.Append(kind).Append('|').Append(script.Name).Append('|').Append(script.Text).Append('\n');
                }
            }

            foreach (var entry in Refactors)
            {
                text.Append("refactor|").Append(entry.Id).Append('|').Append(entry.Table).Append('|').Append(entry.Sql).Append('\n');
            }

            return Convert.ToHexString(SHA512.HashData(Encoding.UTF8.GetBytes(text.ToString())));
        }
    }
}
