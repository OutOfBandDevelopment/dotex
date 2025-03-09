using OoBDev.System.Configuration;
using System.ComponentModel.DataAnnotations;

namespace OoBDev.DacPacCompiler.Cli;

public enum DacPackTools
{
    Unknown = 0,
    SqlClr = 1,
    Merge = 1,
}

public class DacPacBuilderEngineOptions
{
    [CommandParameter(Value = "tool")]
    public DacPackTools Tool { get; set; } = DacPackTools.SqlClr;

    [CommandParameter(Value = "version")]
    public string? ProjectVersion { get; set; }

    [CommandParameter(Value = "project")]
    public string? ProjectName { get; set; }

    [CommandParameter(Value = "dacpac")]
    public string? DacpacFile { get; set; }

    [CommandParameter(Value = "pdb")]
    public string? AssemblyPdbFramework { get; set; }


    [CommandParameter(Value = "dotnet")]
    public required string AssemblyFileNet { get; set; }

    [CommandParameter(Value = "sqlclr")]
    public required string AssemblyFileFramework { get; set; }
}
