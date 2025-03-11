// Ignore Spelling: Dac

using Microsoft.SqlServer.Dac.Model;
using OoBDev.DacFx;
using System;

namespace OoBDev.Microsoft.SqlServer.DacFx;

public class DacPacMergeTemplate : IDacPacMergeTemplate
{
    public string SourcePath { get; set; } = Environment.CurrentDirectory;
    public string[] SourcePatterns { get; set; } = ["*.dacpac"];

    public string TargetPath { get; set; } = default!;

    public SqlServerVersion ServerVersion { get; set; } = SqlServerVersion.SqlAzure;

    public ModelOptionSource ModelOptionSource { get; set; } = ModelOptionSource.Custom;
    public TSqlModelOptions? ModelOptions { get; set; } = new TSqlModelOptions();

    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Version { get; set; }
    public string? BuildVersion { get; set; }
}
