using OoBDev.System.Configuration;
using System.ComponentModel.DataAnnotations;

namespace OoBDev.DacPacCompiler.Cli;

public class DacPacBuilderEngineOptions
{
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
