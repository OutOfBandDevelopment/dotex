namespace OoBDev.DacFx;

public interface IDacPacCompilerConfig
{
    string? TemplatePath { get; }
    string? SourcePath { get; }

    ModelOptionSource? ModelOptionSource { get; }
    string? Description { get; }
    string? Name { get; }
    string? BuildVersion { get; }
    string? Version { get; }
    string? TargetPath { get; }
    string[]? SourcePatterns { get; }
}