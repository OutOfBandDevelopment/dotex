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
    Merge = 2,
}
