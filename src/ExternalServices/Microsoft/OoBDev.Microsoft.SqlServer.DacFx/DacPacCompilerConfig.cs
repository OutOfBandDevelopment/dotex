using Microsoft.Extensions.Configuration;
using OoBDev.DacFx;
using OoBDev.System.Reflection;
using System.Collections.Generic;

namespace OoBDev.Microsoft.SqlServer.DacFx;

public class DacPacCompilerConfig : IDacPacCompilerConfig
{
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


    public const string TemplatePathKey = "DacPac:Template:Path";

    public const string SourcePathKey = "DacPac:Source:Path";
    public const string SourcePatternsKey = "DacPac:Source:Patterns";

    public const string TargetPathKey = "DacPac:Target:Path";
    public const string TargetDescriptionKey = "DacPac:Target:Description";
    public const string TargetNameKey = "DacPac:Target:Name";
    public const string TargetBuildVersionKey = "DacPac:Target:BuildVersion";
    public const string TargetVersionKey = "DacPac:Target:Version";

    public const string ModelOptionSourceKey = "DacPac:Setting:ModelOptionSource";

    public string? TemplatePath { get; }

    public string? SourcePath { get; }
    public string[]? SourcePatterns { get; }

    public string? TargetPath { get; }

    public string? Description { get; }
    public string? Name { get; }
    public string? BuildVersion { get; }
    public string? Version { get; }

    public ModelOptionSource? ModelOptionSource { get; }

    public static Dictionary<string, string> CommandLineSwitchMappings => new Dictionary<string, string>
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
