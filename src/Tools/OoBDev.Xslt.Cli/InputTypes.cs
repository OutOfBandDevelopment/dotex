namespace OoBDev.Xslt.Cli;

/// <summary>How an input file is turned into an XPath-navigable document.</summary>
public enum InputTypes
{
    /// <summary>No mapping.</summary>
    Unknown = 0,

    /// <summary>XML.</summary>
    Xml,

    /// <summary>HTML.</summary>
    Html,

    /// <summary>JSON.</summary>
    Json,

    /// <summary>YAML.</summary>
    Yaml,

    /// <summary>C# source.</summary>
    CSharp = 256,

    /// <summary>Visual Basic source.</summary>
    VB,

    /// <summary>MSBuild structured log (binlog).</summary>
    MSBuildStructuredLog,

    /// <summary>SQL Server data-tier package.</summary>
    DacPac,

    /// <summary>A file-system path, navigated as a directory tree.</summary>
    Path = 512,

    /// <summary>Choose by file extension.</summary>
    ByExtension = 0x7fffffff,
}
