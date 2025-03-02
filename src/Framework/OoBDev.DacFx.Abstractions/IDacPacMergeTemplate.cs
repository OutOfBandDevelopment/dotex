using Microsoft.SqlServer.Dac.Model;

namespace OoBDev.DacFx;

public interface IDacPacMergeTemplate
{
    string? BuildVersion { get; }
    string? Description { get; }
    TSqlModelOptions? ModelOptions { get; }
    ModelOptionSource ModelOptionSource { get; }
    string? Name { get; }
    SqlServerVersion ServerVersion { get; }
    string SourcePath { get; }
    string[] SourcePatterns { get; }
    string TargetPath { get; }
    string? Version { get; }
}