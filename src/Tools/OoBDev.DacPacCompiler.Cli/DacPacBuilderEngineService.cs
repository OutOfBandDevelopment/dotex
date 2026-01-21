// Ignore Spelling: Dac

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OoBDev.DacFx;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.DacPacCompiler.Cli;

/// <summary>
/// Hosted service that builds DACPAC files from SQL CLR assemblies.
/// </summary>
public class DacPacBuilderEngineService : IHostedService
{
    private readonly ILogger _log;
    private readonly IOptions<DacPacBuilderEngineOptions> _settings;
    private readonly IDacPacBuilder _builder;

    /// <summary>
    /// Initializes a new instance of the <see cref="DacPacBuilderEngineService"/> class.
    /// </summary>
    /// <param name="log">The logger for diagnostics.</param>
    /// <param name="settings">The builder configuration options.</param>
    /// <param name="builder">The DACPAC builder implementation.</param>
    public DacPacBuilderEngineService(
        ILogger<DacPacBuilderEngineService> log,
        IOptions<DacPacBuilderEngineOptions> settings,
        IDacPacBuilder builder
        )
    {
        _log = log;
        _settings = settings;
        _builder = builder;
    }

    /// <summary>
    /// Starts the DACPAC build process.
    /// </summary>
    /// <param name="cancellationToken">Token to signal cancellation.</param>
    /// <returns>A completed task.</returns>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        /*
        --sqlclr "$(SolutionDir)Extensions\OoBDev.Data.Vectors\bin\Debug\net481\OoBDev.Data.Vectors.dll" 
        --dotnet "$(SolutionDir)Extensions\OoBDev.Data.Vectors\bin\Debug\net10.0\OoBDev.Data.Vectors.dll"
        */
        _log.LogInformation("AssemblyFileFramework: {AssemblyFileFramework}", _settings.Value.AssemblyFileFramework);
        _log.LogInformation("AssemblyPdbFramework: {AssemblyPdbFramework}", _settings.Value.AssemblyPdbFramework);
        _log.LogInformation("DacpacFile: {DacpacFile}", _settings.Value.DacpacFile);
        _log.LogInformation("ProjectName: {ProjectName}", _settings.Value.ProjectName);
        _log.LogInformation("ProjectVersion: {ProjectVersion}", _settings.Value.ProjectVersion);

        _builder.BuildDacPac(
            assemblyFileFramework: _settings.Value.AssemblyFileFramework,
            assemblyPdbFramework: _settings.Value.AssemblyPdbFramework,
            dacpacFile: _settings.Value.DacpacFile,
            projectName: _settings.Value.ProjectName,
            projectVersion: _settings.Value.ProjectVersion
            );

        return Task.CompletedTask;
    }

    /// <summary>
    /// Stops the service (no-op).
    /// </summary>
    /// <param name="cancellationToken">Token to signal cancellation.</param>
    /// <returns>A completed task.</returns>
    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
