// Ignore Spelling: Dac

using Microsoft.SqlServer.Dac;
using Microsoft.SqlServer.Dac.Model;
using OoBDev.DacFx;
using System.Collections.Generic;

namespace OoBDev.Microsoft.SqlServer.DacFx;

public class DacPacMergeDefinition : IDacPacMergeDefinition
{
    public required string TargetPath { get; set; }
    public SqlServerVersion ServerVersion { get; set; } = SqlServerVersion.Sql160;
    public TSqlModelOptions ModelOptions { get; set; } = new ();

    public IEnumerable<string> SourceFiles { get; set; } = [];

    public string? TargetBuildVersion { get; set; }
    public required PackageMetadata TargetPackageMetadata { get; set; }
}
