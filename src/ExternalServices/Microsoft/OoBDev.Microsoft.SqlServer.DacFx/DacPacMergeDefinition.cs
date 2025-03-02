using Microsoft.SqlServer.Dac;
using Microsoft.SqlServer.Dac.Model;
using OoBDev.DacFx;
using System.Collections.Generic;
using System.Linq;

namespace OoBDev.Microsoft.SqlServer.DacFx;

public class DacPacMergeDefinition : IDacPacMergeDefinition
{
    public string TargetPath { get; set; }
    public SqlServerVersion ServerVersion { get; set; } = SqlServerVersion.Sql160;
    public TSqlModelOptions ModelOptions { get; set; } = new ();

    public IEnumerable<string> SourceFiles { get; set; } = [];

    public string? TargetBuildVersion { get; set; }
    public PackageMetadata TargetPackageMetadata { get; set; }
}
