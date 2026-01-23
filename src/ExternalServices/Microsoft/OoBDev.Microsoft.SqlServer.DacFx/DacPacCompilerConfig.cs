using Microsoft.Extensions.Configuration;
using OoBDev.DacFx;
using OoBDev.System.Reflection;
using System.Collections.Generic;

namespace OoBDev.Microsoft.SqlServer.DacFx;

/// <summary>
/// Provides configuration settings for DacPac compilation from <see cref="IConfiguration"/>.
/// </summary>
public class DacPacCompilerConfig : IDacPacCompilerConfig
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DacPacCompilerConfig"/> class from configuration.
    /// </summary>
    /// <param name="config">The configuration source containing DacPac settings.</param>
    public DacPacCompilerConfig(
        IConfiguration config
        )
    {
        TemplatePath = config[TemplatePathKey];

        SourcePath = config[SourcePathKey];
        SourcePatterns = config[SourcePatternsKey]?.Split(';');

        TargetPath = config[TargetPathKey];
        Description = config[TargetDescriptionKey];
        Name = config[TargetNameKey];
        BuildVersion = config[TargetBuildVersionKey];
        Version = config[TargetVersionKey];

        ModelOptionSource = config[ModelOptionSourceKey].ToEnum<ModelOptionSource>();
    }

    /// <summary>
    /// Configuration key for the template file path.
    /// </summary>
    public const string TemplatePathKey = "DacPac:Template:Path";

    /// <summary>
    /// Configuration key for the source directory path.
    /// </summary>
    public const string SourcePathKey = "DacPac:Source:Path";

    /// <summary>
    /// Configuration key for the source file patterns (semicolon-separated).
    /// </summary>
    public const string SourcePatternsKey = "DacPac:Source:Patterns";

    /// <summary>
    /// Configuration key for the target DacPac output path.
    /// </summary>
    public const string TargetPathKey = "DacPac:Target:Path";

    /// <summary>
    /// Configuration key for the target package description.
    /// </summary>
    public const string TargetDescriptionKey = "DacPac:Target:Description";

    /// <summary>
    /// Configuration key for the target package name.
    /// </summary>
    public const string TargetNameKey = "DacPac:Target:Name";

    /// <summary>
    /// Configuration key for the target build version.
    /// </summary>
    public const string TargetBuildVersionKey = "DacPac:Target:BuildVersion";

    /// <summary>
    /// Configuration key for the target package version.
    /// </summary>
    public const string TargetVersionKey = "DacPac:Target:Version";

    /// <summary>
    /// Configuration key for the model options source setting.
    /// </summary>
    public const string ModelOptionSourceKey = "DacPac:Setting:ModelOptionSource";

    /// <inheritdoc/>
    public string? TemplatePath { get; }

    /// <inheritdoc/>
    public string? SourcePath { get; }

    /// <inheritdoc/>
    public string[]? SourcePatterns { get; }

    /// <inheritdoc/>
    public string? TargetPath { get; }

    /// <inheritdoc/>
    public string? Description { get; }

    /// <inheritdoc/>
    public string? Name { get; }

    /// <inheritdoc/>
    public string? BuildVersion { get; }

    /// <inheritdoc/>
    public string? Version { get; }

    /// <inheritdoc/>
    public ModelOptionSource? ModelOptionSource { get; }

    /// <summary>
    /// Gets the command-line switch mappings for configuration.
    /// </summary>
    public static Dictionary<string, string> CommandLineSwitchMappings => new()
    {
        { "--template",TemplatePathKey},
        { "-t",TemplatePathKey},

        { "--source-path",SourcePathKey},
        { "-s",SourcePathKey},

        { "--source-patterns",SourcePatternsKey},
        { "-p",SourcePatternsKey},

        { "--target-path",TargetPathKey},
        { "-r",TargetPathKey},


        { "--description",TargetDescriptionKey},
        { "-d",TargetDescriptionKey},
        { "--name",TargetNameKey},
        { "-n",TargetNameKey},
        { "--build-version",TargetBuildVersionKey},
        { "-b",TargetBuildVersionKey},
        { "--version",TargetVersionKey},
        { "-v",TargetVersionKey},

        { "--setting-modeloptionsource",ModelOptionSourceKey },
    };
}
