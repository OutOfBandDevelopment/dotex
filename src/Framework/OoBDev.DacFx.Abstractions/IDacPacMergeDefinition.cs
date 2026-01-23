using Microsoft.SqlServer.Dac;
using Microsoft.SqlServer.Dac.Model;
using System.Collections.Generic;

namespace OoBDev.DacFx;

/// <summary>
/// Defines the configuration for merging multiple DacPac files into a single package.
/// </summary>
public interface IDacPacMergeDefinition
{
    /// <summary>
    /// Gets the SQL Server model options to use for the merged package.
    /// </summary>
    TSqlModelOptions ModelOptions { get; }

    /// <summary>
    /// Gets the target SQL Server version for the merged package.
    /// </summary>
    SqlServerVersion ServerVersion { get; }

    /// <summary>
    /// Gets the collection of source DacPac file paths to merge.
    /// </summary>
    IEnumerable<string> SourceFiles { get; }

    /// <summary>
    /// Gets the build version string for the target package.
    /// </summary>
    string? TargetBuildVersion { get; }

    /// <summary>
    /// Gets the metadata for the target DacPac package.
    /// </summary>
    PackageMetadata TargetPackageMetadata { get; }

    /// <summary>
    /// Gets the output file path for the merged DacPac package.
    /// </summary>
    string TargetPath { get; }
}