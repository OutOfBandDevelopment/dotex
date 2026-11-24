using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace OoBDev.System.IO.Ports.Bridge;

/// <summary>Runs one <see cref="BridgeWorker"/> per configured bridge; one failing bridge doesn't stop the others.</summary>
public sealed class BridgeHostedService : BackgroundService
{
    private static readonly TimeSpan RetryInterval = TimeSpan.FromSeconds(5);

    private readonly Ser2NetConfig _config;
    private readonly ISerialPortFactory _serialFactory;
    private readonly ILoggerFactory _loggerFactory;
    private readonly ILogger<BridgeHostedService> _logger;
    private readonly IServiceAdvertiser? _advertiser;

    public BridgeHostedService(
        Ser2NetConfig config,
        ISerialPortFactory serialFactory,
        ILoggerFactory loggerFactory,
        ILogger<BridgeHostedService> logger,
        IServiceAdvertiser? advertiser = null)
    {
        _advertiser = advertiser;
        _config = config;
        _serialFactory = serialFactory;
        _loggerFactory = loggerFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var errors = ConfigValidator.Validate(_config.Bridges);
        if (errors.Count > 0)
        {
            throw new InvalidOperationException("Invalid configuration: " + string.Join(" ", errors));
        }

        if (_config.Bridges.Count == 0)
        {
            _logger.NoBridges();
            await Task.Delay(Timeout.Infinite, stoppingToken).ConfigureAwait(false);
            return;
        }

        await Task.WhenAll(_config.Bridges.Select(b => RunBridgeAsync(b, stoppingToken))).ConfigureAwait(false);
    }

    private async Task RunBridgeAsync(BridgeOptions options, CancellationToken stoppingToken)
    {
        var logger = _loggerFactory.CreateLogger($"Bridge.{options.PortLabel}");
        try
        {
            using var worker = new BridgeWorker(options, _serialFactory, logger, RetryInterval, _advertiser);
            await worker.RunAsync(stoppingToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // Shutdown.
        }
        catch (Exception ex)
        {
            logger.BridgeFailed(ex, options.PortLabel);
        }
    }
}
