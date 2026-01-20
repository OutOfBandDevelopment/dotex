using OoBDev.DataLoader.Models;

namespace OoBDev.DataLoader
{
    public interface IDatabaseDeploymentTemplate
    {
        string? AssemblySearchPath { get; }
        string? BasePath { get; }
        string? ConnectionString { get; }

        string[] Contexts { get; }
        DuplicateHandling DuplicateHandling { get; }
        string[] SourceDataPaths { get; }
        DataloaderUserSession UserSession { get; }
        ImportHandling ImportMasterData { get; }
        bool DisableForeignKeysBeforeInsert { get; }
    }
}