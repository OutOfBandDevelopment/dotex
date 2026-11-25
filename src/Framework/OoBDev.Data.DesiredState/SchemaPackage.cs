using System;
using System.Collections.Generic;
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
