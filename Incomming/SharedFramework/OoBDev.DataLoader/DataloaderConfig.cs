using OoBDev.Extensions;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace OoBDev.DataLoader
{
    public class DataloaderConfig : IDataloaderConfig
    {
        public DataloaderConfig(
            IConfiguration config
            )
        {
            DeploymentDescriptionFile = config[DeploymentDescriptionFileKey];
            DeploymentBasePath = config[DeploymentBasePathKey];
            DeploymentUserName = config[DeploymentUserNameKey];
            DeploymentUserId = config[DeploymentUserIdKey].ToGuid();

            TargetDatabaseConnectionString = config[TargetDatabaseConnectionStringKey];
            TargetContexts = config[TargetContextsKey]?.Split(';');

            SourceAssemblySearchPath = config[SourceAssemblySearchPathKey];
            SourceDataSearchPaths = config[SourceDataSearchPathsKey]?.Split(';');

            SettingsImportMasterData = config[SettingsImportMasterDataKey].ToEnum<ImportHandling>();
            SettingsDuplicateHandling = config[SettingsDuplicateHandlingKey].ToEnum<DuplicateHandling>();
            SettingsDisableForeignKeysBeforeInsert = config[SettingsDisableForeignKeysBeforeInsertKey].ToBooleanOrNull();
        }

        public const string DeploymentDescriptionFileKey = "Dataloader:Deployment:Description:File";
        public const string DeploymentBasePathKey = "Dataloader:Deployment:BasePath";
        public const string DeploymentUserNameKey = "Dataloader:Deployment:User:Name";
        public const string DeploymentUserIdKey = "Dataloader:Deployment:User:Id";

        public const string TargetDatabaseConnectionStringKey = "Dataloader:Target:Database:ConnectionString";
        public const string TargetContextsKey = "Dataloader:Target:Contexts";

        public const string SourceAssemblySearchPathKey = "Dataloader:Source:Assembly:SearchPath";
        public const string SourceDataSearchPathsKey = "Dataloader:Source:Data:SearchPaths";

        public const string SettingsImportMasterDataKey = "Dataloader:Settings:ImportMasterData";
        public const string SettingsDuplicateHandlingKey = "Dataloader:Settings:DuplicateHandling";
        public const string SettingsDisableForeignKeysBeforeInsertKey = "Dataloader:Settings:DisableForeignKeysBeforeInsert";

        public string? DeploymentDescriptionFile { get; }
        public string? DeploymentBasePath { get; }
        public string? DeploymentUserName { get; }
        public Guid? DeploymentUserId { get; }

        public string? TargetDatabaseConnectionString { get; }
        public string?[]? TargetContexts { get; }

        public string? SourceAssemblySearchPath { get; }
        public string?[]? SourceDataSearchPaths { get; }

        public ImportHandling? SettingsImportMasterData { get; }
        public DuplicateHandling? SettingsDuplicateHandling { get; }
        public bool? SettingsDisableForeignKeysBeforeInsert { get; }

        public static Dictionary<string, string> CommandLineSwitchMappings => new Dictionary<string, string>
        {
                { "--template",DeploymentDescriptionFileKey},
                { "-t",DeploymentDescriptionFileKey},

                { "--base-path",DeploymentBasePathKey},
                { "-b",DeploymentBasePathKey},

                { "--user-name",DeploymentUserNameKey},
                { "--user-id",DeploymentUserIdKey},

                { "--connection-string",TargetDatabaseConnectionStringKey},
                { "--connectionstring",TargetDatabaseConnectionStringKey},
                { "-c",TargetDatabaseConnectionStringKey},

                { "--contexts",TargetContextsKey},
                { "-x",TargetContextsKey},

                { "--assembly-path",SourceAssemblySearchPathKey},
                { "-a",SourceAssemblySearchPathKey},

                { "--data-paths",SourceDataSearchPathsKey},
                { "-d",SourceDataSearchPathsKey},

                { "--setting-masterdata",SettingsImportMasterDataKey},
                { "--setting-duplicate-handling",SettingsDuplicateHandlingKey},
                { "--setting-disable-fk",SettingsDisableForeignKeysBeforeInsertKey},
        };
    }
}
