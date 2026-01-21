using OoBDev.System.Configuration;

namespace OoBDev.DacPacCompiler.Cli;

/// <summary>
/// Available DACPAC compilation tools.
/// </summary>
public enum DacPackTools
{
    /// <summary>
    /// Unknown tool type.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// SQL CLR assembly packaging tool.
    /// </summary>
    SqlClr = 1,

    /// <summary>
    /// DACPAC merge tool.
    /// </summary>
    Merge = 1,
}

/// <summary>
/// Configuration options for the DACPAC builder engine.
/// </summary>
public class DacPacBuilderEngineOptions
{
    /// <summary>
    /// Gets or sets the tool to use for DACPAC compilation.
    /// </summary>
    [CommandParameter(Value = "tool")]
    public DacPackTools Tool { get; set; } = DacPackTools.SqlClr;

    /// <summary>
    /// Gets or sets the project version.
    /// </summary>
    [CommandParameter(Value = "version")]
    public string? ProjectVersion { get; set; }

    /// <summary>
    /// Gets or sets the project name.
    /// </summary>
    [CommandParameter(Value = "project")]
    public string? ProjectName { get; set; }

    /// <summary>
    /// Gets or sets the output DACPAC file path.
    /// </summary>
    [CommandParameter(Value = "dacpac")]
    public string? DacpacFile { get; set; }

    /// <summary>
    /// Gets or sets the assembly PDB file path for framework assemblies.
    /// </summary>
    [CommandParameter(Value = "pdb")]
    public string? AssemblyPdbFramework { get; set; }

    /// <summary>
    /// Gets or sets the SQL CLR assembly file path.
    /// </summary>
    [CommandParameter(Value = "sqlclr")]
    public required string AssemblyFileFramework { get; set; }
}
