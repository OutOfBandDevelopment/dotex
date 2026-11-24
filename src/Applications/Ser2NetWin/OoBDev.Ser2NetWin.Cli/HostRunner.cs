using OoBDev.System.IO.Ports.Bridge;
using OoBDev.System.IO.Usb.Bridge;
using OoBDev.System.IO.UsbHids;
using OoBDev.System.IO.UsbTmc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace OoBDev.Ser2NetWin.Cli;

/// <summary>Runs the bridges either in the console (Ctrl+C to stop) or under the Service Control Manager.</summary>
public static class HostRunner
{
    public static async Task<int> RunAsync(string configPath)
    {
        var config = new ConfigStore(configPath).Load();

        var builder = Host.CreateApplicationBuilder(new HostApplicationBuilderSettings
        {
            ContentRootPath = AppContext.BaseDirectory,
        });

        // No-op when started from a console; wires up SCM lifetime and Event Log when started as a service.
        builder.Services.AddWindowsService(options => options.ServiceName = ServiceInstaller.ServiceName);
        builder.Services.AddSingleton(config);
        builder.Services.AddSingleton<ISerialPortFactory, SystemSerialPortFactory>();
        if (config.Mdns.Enabled)
        {
            builder.Services.AddSingleton<IServiceAdvertiser>(new MdnsAdvertiser(config.Mdns));
        }

        builder.Services.AddHostedService<BridgeHostedService>();

        var usbBridges = UsbBridgeConfig.Load(configPath);
        if (usbBridges.Count > 0)
        {
            builder.Services.AddSingleton<IUsbTmcDeviceFactory, SystemUsbTmcDeviceFactory>();
            builder.Services.AddSingleton<IUsbHidDeviceFactory, SystemUsbHidDeviceFactory>();
            builder.Services.AddHostedService(sp => new UsbBridgeHostedService(
                usbBridges,
                sp.GetRequiredService<IUsbTmcDeviceFactory>(),
                sp.GetRequiredService<IUsbHidDeviceFactory>(),
                sp.GetRequiredService<ILoggerFactory>()));
        }

        using var host = builder.Build();
        await host.RunAsync().ConfigureAwait(false);
        return 0;
    }
}
