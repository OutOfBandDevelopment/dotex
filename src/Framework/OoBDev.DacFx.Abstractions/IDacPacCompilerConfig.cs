namespace OoBDev.DacFx;

/// <summary>
/// Represents configuration settings for the DacPac compiler.
/// </summary>
public interface IDacPacCompilerConfig
{
    /// <summary>
    /// Gets the path to the template file to use for compilation.
    /// </summary>
    string? TemplatePath { get; }

    /// <summary>
    /// Gets the path to the source directory containing SQL files.
    /// </summary>
    string? SourcePath { get; }

    /// <summary>
    /// Gets the source for SQL model options when merging DacPac files.
    /// </summary>
    ModelOptionSource? ModelOptionSource { get; }

    /// <summary>
    /// Gets the description of the DacPac package.
    /// </summary>
    string? Description { get; }

    /// <summary>
    /// Gets the name of the DacPac package.
    /// </summary>
    string? Name { get; }

    /// <summary>
    /// Gets the build version string for the package.
    /// </summary>
    string? BuildVersion { get; }

    /// <summary>
    /// Gets the semantic version of the package.
    /// </summary>
    string? Version { get; }

    /// <summary>
    /// Gets the output path for the compiled DacPac file.
    /// </summary>
    string? TargetPath { get; }

    /// <summary>
    /// Gets the file patterns to include when searching for source files.
    /// </summary>
    string[]? SourcePatterns { get; }
}