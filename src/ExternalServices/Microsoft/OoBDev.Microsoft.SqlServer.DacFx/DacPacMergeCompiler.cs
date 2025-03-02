using Microsoft.Extensions.Logging;
using Microsoft.SqlServer.Dac;
using Microsoft.SqlServer.Dac.Model;
using OoBDev.DacFx;
using System.IO;
using System.Text;

namespace OoBDev.Microsoft.SqlServer.DacFx;

public class DacPacMergeCompiler : IDacPacMergeCompiler
{
    private readonly ILogger<DacPacMergeCompiler> _logger;

    public DacPacMergeCompiler(ILogger<DacPacMergeCompiler> logger)
    {
        _logger = logger;
    }

    public void CreatePackage(IDacPacMergeDefinition def)
    {

        // ===== TSqlModel =====

        _logger.LogInformation($"Create: {def.TargetPath}");

        var target = new TSqlModel(def.ServerVersion, def.ModelOptions);

        var preDeploymentScript = new StringBuilder();
        var postDeploymentScript = new StringBuilder();

        foreach (var file in def.SourceFiles)
        {
            _logger.LogInformation($"Read: {file}");

            using var sqlModel = DacTools.OpenDacPacModel(file);
            foreach (var source in sqlModel.ReadPackage())
            {
                _logger.LogInformation($"{file}:{source.name}");
                target.AddOrUpdateObjects(source.script, source.name, new TSqlObjectOptions()); //TODO: figure out how to carry TSqlObjectOptions forward
            }

            var scripts = DacTools.GetScripts(file);
            if (!string.IsNullOrWhiteSpace(scripts.PreDeploymentScript)) preDeploymentScript.AppendLine(scripts.PreDeploymentScript);
            if (!string.IsNullOrWhiteSpace(scripts.PostDeploymentScript)) postDeploymentScript.AppendLine(scripts.PostDeploymentScript);
        }

        if (!string.IsNullOrWhiteSpace(def.TargetBuildVersion))
            postDeploymentScript.AppendLine(DacTools.GenerateBuildVersionScript(def.TargetBuildVersion));

        _logger.LogInformation($"Build: {def.TargetPath}");

        var parentPath = Path.GetDirectoryName(def.TargetPath);
        if (!Directory.Exists(parentPath)) Directory.CreateDirectory(parentPath);

        DacPackageExtensions.BuildPackage(def.TargetPath, target, def.TargetPackageMetadata);

        _logger.LogInformation($"Add scripts: {def.TargetPath}");
        DacTools.AddScripts(def.TargetPath, (preDeploymentScript.ToString(), postDeploymentScript.ToString()));

        _logger.LogInformation($"Complete: {def.TargetPath}");
    }

}
