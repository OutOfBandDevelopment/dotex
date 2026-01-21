using Microsoft.SqlServer.Dac.Model;

namespace OoBDev.DacFx;

/// <summary>
/// Represents a template for configuring DacPac merge operations.
/// </summary>
public interface IDacPacMergeTemplate
{
    /// <summary>
    /// Gets the build version string for the merged package.
    /// </summary>
    string? BuildVersion { get; }

    /// <summary>
    /// Gets the description of the merged DacPac package.
    /// </summary>
    string? Description { get; }

    /// <summary>
    /// Gets the SQL Server model options to use for the merge, if using custom options.
    /// </summary>
    TSqlModelOptions? ModelOptions { get; }

    /// <summary>
    /// Gets the source for determining which model options to use during merge.
    /// </summary>
    ModelOptionSource ModelOptionSource { get; }

    /// <summary>
    /// Gets the name of the merged DacPac package.
    /// </summary>
    string? Name { get; }

    /// <summary>
    /// Gets the target SQL Server version for the merged package.
    /// </summary>
    SqlServerVersion ServerVersion { get; }

    /// <summary>
    /// Gets the source directory path containing DacPac files to merge.
    /// </summary>
    string SourcePath { get; }

    /// <summary>
    /// Gets the file patterns to use when searching for source DacPac files.
    /// </summary>
    string[] SourcePatterns { get; }

    /// <summary>
    /// Gets the output file path for the merged DacPac package.
    /// </summary>
    string TargetPath { get; }

    /// <summary>
    /// Gets the semantic version of the merged package.
    /// </summary>
    string? Version { get; }
}