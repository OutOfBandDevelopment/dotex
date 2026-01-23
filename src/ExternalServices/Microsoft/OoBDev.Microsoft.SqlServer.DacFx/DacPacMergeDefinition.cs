// Ignore Spelling: Dac

using Microsoft.SqlServer.Dac;
using Microsoft.SqlServer.Dac.Model;
using OoBDev.DacFx;
using System.Collections.Generic;

namespace OoBDev.Microsoft.SqlServer.DacFx;

/// <summary>
/// Defines the configuration for merging multiple DacPac files into a single package.
/// </summary>
public class DacPacMergeDefinition : IDacPacMergeDefinition
{
    /// <inheritdoc/>
    public required string TargetPath { get; set; }

    /// <inheritdoc/>
    public SqlServerVersion ServerVersion { get; set; } = SqlServerVersion.Sql160;

    /// <inheritdoc/>
    public TSqlModelOptions ModelOptions { get; set; } = new ();

    /// <inheritdoc/>
    public IEnumerable<string> SourceFiles { get; set; } = [];

    /// <inheritdoc/>
    public string? TargetBuildVersion { get; set; }

    /// <inheritdoc/>
    public required PackageMetadata TargetPackageMetadata { get; set; }
}
