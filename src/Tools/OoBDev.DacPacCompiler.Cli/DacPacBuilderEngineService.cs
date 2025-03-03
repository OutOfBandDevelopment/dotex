using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OoBDev.DacFx;
using OoBDev.System.Text.Templating;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.DacPacCompiler.Cli;

public class DacPacBuilderEngineService : IHostedService
{
    private readonly ILogger _log;
    private readonly IOptions<DacPacBuilderEngineOptions> _settings;
    private readonly IServiceProvider _serviceProvider;

    public DacPacBuilderEngineService(
        ILogger<DacPacBuilderEngineService> log,
        IOptions<DacPacBuilderEngineOptions> settings,
        IServiceProvider serviceProvider
        )
    {
        _log = log;
        _settings = settings;
        _serviceProvider = serviceProvider;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var builder = new DacPacBuilder();

        _log.LogInformation("AssemblyFileFramework: {AssemblyFileFramework}", _settings.Value.AssemblyFileFramework);
        _log.LogInformation("AssemblyFileNet: {AssemblyFileNet}", _settings.Value.AssemblyFileNet);
        _log.LogInformation("AssemblyPdbFramework: {AssemblyPdbFramework}", _settings.Value.AssemblyPdbFramework);
        _log.LogInformation("DacpacFile: {DacpacFile}", _settings.Value.DacpacFile);
        _log.LogInformation("ProjectName: {ProjectName}", _settings.Value.ProjectName);
        _log.LogInformation("ProjectVersion: {ProjectVersion}", _settings.Value.ProjectVersion);

        builder.BuildDacPac(
            assemblyFileFramework: _settings.Value.AssemblyFileFramework,
            assemblyFileNet: _settings.Value.AssemblyFileNet,
            assemblyPdbFramework: _settings.Value.AssemblyPdbFramework,
            dacpacFile: _settings.Value.DacpacFile,
            projectName: _settings.Value.ProjectName,
            projectVersion: _settings.Value.ProjectVersion
            );

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
