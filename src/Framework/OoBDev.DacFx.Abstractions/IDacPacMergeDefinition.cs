using Microsoft.SqlServer.Dac;
using Microsoft.SqlServer.Dac.Model;
using System.Collections.Generic;

namespace OoBDev.DacFx;

public interface IDacPacMergeDefinition
{
    TSqlModelOptions ModelOptions { get; }
    SqlServerVersion ServerVersion { get; }
    IEnumerable<string> SourceFiles { get; }
    string? TargetBuildVersion { get; }
    PackageMetadata TargetPackageMetadata { get; }
    string TargetPath { get; }
}