using System;

namespace OoBDev.DataLoader.Models
{
    public class DatabaseDeploymentTemplate : IDatabaseDeploymentTemplate
    {
        public string? ConnectionString { get; set; }
        public string? BasePath { get; set; }
        public string? AssemblySearchPath { get; set; }

        public DataloaderUserSession UserSession { get; set; } = new DataloaderUserSession();
        public string[] Contexts { get; set; } = Array.Empty<string>();
        public string[] SourceDataPaths { get; set; } = Array.Empty<string>();

        public ImportHandling ImportMasterData { get; set; } = ImportHandling.First;
        public DuplicateHandling DuplicateHandling { get; set; } = DuplicateHandling.Fail;
        public bool DisableForeignKeysBeforeInsert { get; set; } = true;
    }
}