using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BotChat.HostRunner;

public class RunnerHost<TRunner> : IHostedService where TRunner : IRunner
{
    private Task? _task = null;
    private readonly CancellationTokenSource _cancellationTokenSource = new();

    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger _logger;

    public RunnerHost(
        IServiceProvider serviceProvider,
        ILogger<RunnerHost<TRunner>> logger
        )
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (_task is null)
            {
                _task = Task.Run(async () =>
                {
                    var localCancellationToken = _cancellationTokenSource.Token;
                    while (!localCancellationToken.IsCancellationRequested)
                    {
                        using var scope = _serviceProvider.CreateScope();
                        var runner = ActivatorUtilities.CreateInstance<TRunner>(scope.ServiceProvider);
                        _logger.LogInformation("Start Runner: {runner}", typeof(TRunner));
                        await runner.ExecuteAsync(localCancellationToken);
                    }
                }, cancellationToken);
            }
            else
            {
                _logger.LogInformation("Already running: {runner}", typeof(TRunner));
            }
        }
        catch (Exception ex)
        {
            _task = null;
            _logger.LogError("ERROR {message}", ex.Message);
            _logger.LogDebug("ERROR {exception}", ex);
            throw;
        }
        return Task.CompletedTask;
    }
    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_task is not null)
        {
            _logger.LogInformation("Stopping Runner: {runner}", typeof(TRunner));
            await _cancellationTokenSource.CancelAsync();
            await _task;
        }
        else
        {
            _logger.LogInformation("No: {runner}", typeof(TRunner));
        }
    }
}
