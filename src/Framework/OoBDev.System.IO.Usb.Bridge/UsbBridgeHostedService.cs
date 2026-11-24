using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OoBDev.System.IO.UsbHids;
using OoBDev.System.IO.UsbTmc;

namespace OoBDev.System.IO.Usb.Bridge;

/// <summary>Runs one <see cref="UsbBridgeServer"/> per configured USB device; one failing bridge does not stop the others.</summary>
public sealed class UsbBridgeHostedService(
    IReadOnlyList<UsbBridgeOptions> bridges,
    IUsbTmcDeviceFactory tmcFactory,
    IUsbHidDeviceFactory hidFactory,
    ILoggerFactory loggerFactory) : BackgroundService
{
    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var errors = bridges.SelectMany(b => b.Validate()).ToList();
        if (errors.Count > 0)
            throw new InvalidOperationException("Invalid USB bridge configuration: " + string.Join(" ", errors));
        if (bridges.Count == 0)
        {
            await Task.Delay(Timeout.Infinite, stoppingToken).ConfigureAwait(false);
            return;
        }

        await Task.WhenAll(bridges.Select(b => RunAsync(b, stoppingToken))).ConfigureAwait(false);
    }

    private async Task RunAsync(UsbBridgeOptions options, CancellationToken stoppingToken)
    {
        var logger = loggerFactory.CreateLogger($"UsbBridge.{options.DisplayName}");
        try
        {
            using var server = new UsbBridgeServer(options, tmcFactory, hidFactory, logger);
            await server.RunAsync(stoppingToken).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.SessionFailed(ex.Message);
        }
    }
}
