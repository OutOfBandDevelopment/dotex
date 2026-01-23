// Ignore Spelling: Dac

using Microsoft.SqlServer.Dac.Model;
using OoBDev.DacFx;
using System;

namespace OoBDev.Microsoft.SqlServer.DacFx;

/// <summary>
/// Template for configuring DacPac merge operations.
/// </summary>
public class DacPacMergeTemplate : IDacPacMergeTemplate
{
    /// <inheritdoc/>
    public string SourcePath { get; set; } = Environment.CurrentDirectory;

    /// <inheritdoc/>
    public string[] SourcePatterns { get; set; } = ["*.dacpac"];

    /// <inheritdoc/>
    public string TargetPath { get; set; } = default!;

    /// <inheritdoc/>
    public SqlServerVersion ServerVersion { get; set; } = SqlServerVersion.SqlAzure;

    /// <inheritdoc/>
    public ModelOptionSource ModelOptionSource { get; set; } = ModelOptionSource.Custom;

    /// <inheritdoc/>
    public TSqlModelOptions? ModelOptions { get; set; } = new TSqlModelOptions();

    /// <inheritdoc/>
    public string? Name { get; set; }

    /// <inheritdoc/>
    public string? Description { get; set; }

    /// <inheritdoc/>
    public string? Version { get; set; }

    /// <inheritdoc/>
    public string? BuildVersion { get; set; }
}
