using Microsoft.Extensions.FileSystemGlobbing;
using Microsoft.SqlServer.Dac;
using Microsoft.SqlServer.Dac.Model;
using OoBDev.DacFx;
using System;
using System.IO;
using System.Linq;

namespace OoBDev.Microsoft.SqlServer.DacFx;

public class DacPacMergeDefinitionFactory : IDacPacMergeDefinitionFactory
{
    public IDacPacMergeDefinition Create(IDacPacMergeTemplate template)
    {
        var matcher = new Matcher();
        matcher.AddIncludePatterns(template.SourcePatterns.Where(i => !i.StartsWith('!')));
        matcher.AddExcludePatterns(template.SourcePatterns.Where(i => i.StartsWith('!')).Select(i => i[1..]));
        var files = matcher.GetResultsInFullPath(template.SourcePath).ToArray();

        var def = new DacPacMergeDefinition()
        {

            SourceFiles = files,

            ModelOptions = template.ModelOptionSource switch
            {
                ModelOptionSource.First => DacTools.GetModelOptions(files.First()),
                ModelOptionSource.Last => DacTools.GetModelOptions(files.Last()),
                _ => template.ModelOptions,
            } ?? new TSqlModelOptions(),

            ServerVersion = template.ServerVersion,
            TargetPath = Path.GetFullPath(template.TargetPath),

            TargetBuildVersion = string.IsNullOrWhiteSpace(template.BuildVersion) ? template.Version : template.BuildVersion,

            TargetPackageMetadata = new PackageMetadata
            {
                Description = string.IsNullOrWhiteSpace(template.Description) ? DateTime.Now.ToString() : template.Description,
                Name = string.IsNullOrWhiteSpace(template.Name) ? Path.GetFileNameWithoutExtension(template.TargetPath) : template.Name,
                Version = template.Version ?? "0.0.0",
            }
        };
        return def;
    }
}
