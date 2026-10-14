using Microsoft.Extensions.Logging;
using Microsoft.SqlServer.Dac;
using Microsoft.SqlServer.Dac.Model;
using OoBDev.DacFx;
using System.IO;
using System.Text;

namespace OoBDev.Microsoft.SqlServer.DacFx;

/// <summary>
/// Compiles multiple DacPac files into a single merged DacPac package.
/// </summary>
public partial class DacpacMergeCompiler : IDacpacMergeCompiler
{
    private readonly ILogger<DacpacMergeCompiler> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DacpacMergeCompiler"/> class.
    /// </summary>
    /// <param name="logger">The logger for compilation operations.</param>
    public DacpacMergeCompiler(ILogger<DacpacMergeCompiler> logger) => _logger = logger;

    /// <inheritdoc/>
    public void CreatePackage(IDacPacMergeDefinition def)
    {
        // ===== TSqlModel =====

        LogCreate(def.TargetPath);

        var target = new TSqlModel(def.ServerVersion, def.ModelOptions);

        var preDeploymentScript = new StringBuilder();
        var postDeploymentScript = new StringBuilder();

        foreach (var file in def.SourceFiles)
        {
            LogRead(file);

            using var sqlModel = DacTools.OpenDacPacModel(file);
            foreach (var source in sqlModel.ReadPackage())
            {
                LogMessage(file, source.name);
                target.AddOrUpdateObjects(source.script, source.name, new TSqlObjectOptions()); //TODO: figure out how to carry TSqlObjectOptions forward
            }

            var scripts = DacTools.GetScripts(file);
            if (!string.IsNullOrWhiteSpace(scripts.PreDeploymentScript)) preDeploymentScript.AppendLine(scripts.PreDeploymentScript);
            if (!string.IsNullOrWhiteSpace(scripts.PostDeploymentScript)) postDeploymentScript.AppendLine(scripts.PostDeploymentScript);
        }

        if (!string.IsNullOrWhiteSpace(def.TargetBuildVersion))
            postDeploymentScript.AppendLine(DacTools.GenerateBuildVersionScript(def.TargetBuildVersion));

        LogBuild(def.TargetPath);

        var parentPath = Path.GetDirectoryName(def.TargetPath);
        if (parentPath != null && !Directory.Exists(parentPath)) Directory.CreateDirectory(parentPath);

        DacPackageExtensions.BuildPackage(def.TargetPath, target, def.TargetPackageMetadata);

        LogAddScripts(def.TargetPath);
        DacTools.AddScripts(def.TargetPath, (preDeploymentScript.ToString(), postDeploymentScript.ToString()));

        LogComplete(def.TargetPath);
    }

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "Create: {TargetPath}")]
    private partial void LogCreate(string? targetPath);

    [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "Read: {file}")]
    private partial void LogRead(string? file);

    [LoggerMessage(EventId = 3, Level = LogLevel.Information, Message = "{File}:{Name}")]
    private partial void LogMessage(string? file, string? name);

    [LoggerMessage(EventId = 4, Level = LogLevel.Information, Message = "Build: {TargetPath}")]
    private partial void LogBuild(string? targetPath);

    [LoggerMessage(EventId = 5, Level = LogLevel.Information, Message = "Add scripts: {TargetPath}")]
    private partial void LogAddScripts(string? targetPath);

    [LoggerMessage(EventId = 6, Level = LogLevel.Information, Message = "Complete: {TargetPath}")]
    private partial void LogComplete(string? targetPath);
}
