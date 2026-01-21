using System;

namespace OoBDev.DataLoader
{
    public interface IDataloaderConfig
    {
        string? DeploymentDescriptionFile { get; }
        string? DeploymentBasePath { get; }
        string? DeploymentUserName { get; }
        Guid? DeploymentUserId { get; }

        string? TargetDatabaseConnectionString { get; }
        string?[]? TargetContexts { get; }

        string? SourceAssemblySearchPath { get; }
        string?[]? SourceDataSearchPaths { get; }

        ImportHandling? SettingsImportMasterData { get; }
        DuplicateHandling? SettingsDuplicateHandling { get; }
        bool? SettingsDisableForeignKeysBeforeInsert { get; }

    }
}