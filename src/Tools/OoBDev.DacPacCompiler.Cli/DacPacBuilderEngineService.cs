// Ignore Spelling: Dac

using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OoBDev.DacFx;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.DacPacCompiler.Cli;

public class DacPacBuilderEngineService : IHostedService
{
    private readonly ILogger _log;
    private readonly IOptions<DacPacBuilderEngineOptions> _settings;
    private readonly IDacPacBuilder _builder;

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

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
