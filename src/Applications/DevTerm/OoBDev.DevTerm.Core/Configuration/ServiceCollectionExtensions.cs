using Microsoft.Extensions.DependencyInjection.Extensions;
using OoBDev.System.IO;
using OoBDev.System.IO.UsbTmc;
using OoBDev.System.IO.UsbHids;
using OoBDev.System.IO.Tcp;
using OoBDev.System.IO.Ports;
using OoBDev.System.IO.Mqtt;
using OoBDev.System.IO.Brokers;
using OoBDev.System.IO.Bluetooth;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using OoBDev.DevTerm.Hosting;
using OoBDev.Telemetry;
using OoBDev.DevTerm.Presenters.Text;
using OoBDev.DevTerm.Loopback;

namespace OoBDev.DevTerm.Configuration;

/// <summary>Composition of the DevTerm engine, presenters and the transport a <see cref="CliOptions"/> selects.</summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the core engine, text presenters and the transport <paramref name="cliOptions"/> selects, configured from it.
    /// Every front end calls this so they all wire up identically from the same bound options.
    /// </summary>
    public static IServiceCollection AddDevTermFrontEnd(this IServiceCollection services, CliOptions cliOptions)
    {
        services.AddDevTermPresenters(cliOptions);
        StartTelemetry(cliOptions);

        var transport = cliOptions.Transport?.ToLowerInvariant();
        switch (transport)
        {
            case "loopback":
                services.TryAddLoopbackTransport();
                services.Configure<LoopbackTransportOptions>(o => o.SampleIntervalMs = cliOptions.LoopbackSampleIntervalMs);
                break;
            case "remote":
                services.Configure<OoBDev.DevTerm.Remote.RemoteDeviceOptions>(o =>
                {
                    o.BaseUrl = cliOptions.Host ?? string.Empty;
                    o.Connection = cliOptions.RemoteConnection ?? string.Empty;
                    o.Key = cliOptions.Password ?? string.Empty;
                });
                services.TryAddTransient<IDeviceAdapter, OoBDev.DevTerm.Remote.RemoteDeviceAdapter>();
                break;
            case "service":
                services.TryAddTransient<IDeviceAdapter>(_ => new ServiceHostBridge(
                    Enum.Parse<ServiceKind>(cliOptions.Service ?? string.Empty, true),
                    ushort.TryParse(cliOptions.Port, global::System.Globalization.CultureInfo.InvariantCulture, out var servicePort) ? servicePort : (ushort)0,
                    global::System.Net.IPAddress.TryParse(cliOptions.Host, out var serviceAddress) ? serviceAddress : null));
                break;
            case "tcp":
                services.AddTcpEndpointDeviceAdapter();
                services.Configure<TcpEndpointOptions>(o =>
                {
                    o.Mode = cliOptions.Listen ? TcpEndpointMode.Listener : TcpEndpointMode.Client;
                    o.Host = cliOptions.Host;
                    o.Port = int.TryParse(cliOptions.Port, global::System.Globalization.CultureInfo.InvariantCulture, out var tcpPort) ? tcpPort : 0;
                    o.WriteTimeoutMs = cliOptions.WriteTimeoutMs;
                    o.WriteByteDelayMs = cliOptions.WriteByteDelayMs;
                });
                break;
            case "hid":
                services.AddUsbHidEndpointDeviceAdapter();
                services.Configure<UsbHidEndpointOptions>(o =>
                {
                    o.VendorId = cliOptions.VendorId;
                    o.ProductId = cliOptions.ProductId;
                    o.SerialNumber = cliOptions.SerialNumber;
                    o.DevicePath = cliOptions.DevicePath;
                });
                break;
            case "usbtmc":
                services.AddUsbTmcDeviceAdapter();
                services.Configure<UsbTmcDeviceAdapterOptions>(o =>
                {
                    o.VendorId = cliOptions.VendorId;
                    o.ProductId = cliOptions.ProductId;
                    o.SerialNumber = cliOptions.SerialNumber;
                    o.DevicePath = cliOptions.DevicePath;
                    o.WriteTimeoutMs = cliOptions.WriteTimeoutMs;
                    o.ReadTimeoutMs = cliOptions.ReadTimeoutMs;
                });
                break;
            case "ble":
                services.AddBleDeviceAdapter();
                services.Configure<BleDeviceAdapterOptions>(o =>
                {
                    o.DeviceId = cliOptions.BleDeviceId ?? string.Empty;
                    if (cliOptions.BleServiceUuid is { Length: > 0 } serviceUuid)
                    {
                        o.ServiceUuid = serviceUuid;
                    }

                    if (cliOptions.BleWriteCharacteristicUuid is { Length: > 0 } writeUuid)
                    {
                        o.WriteCharacteristicUuid = writeUuid;
                    }

                    if (cliOptions.BleNotifyCharacteristicUuid is { Length: > 0 } notifyUuid)
                    {
                        o.NotifyCharacteristicUuid = notifyUuid;
                    }

                    o.WriteTimeoutMs = cliOptions.WriteTimeoutMs;
                });
                BlePlatformAdapterLoader.TryRegisterPlatformAdapter(services);
                break;
            case "rfc2217":
                services.AddRfc2217ClientDeviceAdapter();
                services.Configure<Rfc2217ClientOptions>(o =>
                {
                    o.Host = cliOptions.Host ?? string.Empty;
                    o.Port = int.TryParse(cliOptions.Port, global::System.Globalization.CultureInfo.InvariantCulture, out var rfc2217Port) ? rfc2217Port : 0;
                    o.BaudRate = cliOptions.Baud;
                    o.DataBits = cliOptions.DataBits;
                    o.Parity = cliOptions.Parity;
                    o.StopBits = cliOptions.StopBits;
                    o.DtrEnable = cliOptions.Dtr;
                    o.RtsEnable = cliOptions.Rts;
                    o.WriteTimeoutMs = cliOptions.WriteTimeoutMs;
                    o.WriteByteDelayMs = cliOptions.WriteByteDelayMs;
                });
                break;
            case "vxi11":
                services.AddVxi11DeviceAdapter();
                services.Configure<Vxi11DeviceAdapterOptions>(o =>
                {
                    o.Host = cliOptions.Host ?? string.Empty;
                    o.Port = int.TryParse(cliOptions.Port, global::System.Globalization.CultureInfo.InvariantCulture, out var vxi11Port) ? vxi11Port : 0;
                    o.WriteTimeoutMs = cliOptions.WriteTimeoutMs;
                });
                break;
            case "mqtt":
                services.AddMqttDeviceAdapter();
                services.Configure<MqttDeviceAdapterOptions>(o =>
                {
                    o.Host = cliOptions.Host ?? string.Empty;
                    o.Port = int.TryParse(cliOptions.Port, global::System.Globalization.CultureInfo.InvariantCulture, out var mqttPort) ? mqttPort : 1883;
                    o.SubscribeTopics = Topics(cliOptions.Subscribe);
                    o.PublishTopic = cliOptions.Publish;
                    o.Username = cliOptions.Username;
                    o.Password = cliOptions.Password;
                    o.TimeoutMs = cliOptions.WriteTimeoutMs;
                });
                break;
            case "amqp":
            case "stomp":
                if (transport == "amqp")
                {
                    services.AddAmqpDeviceAdapter();
                }
                else
                {
                    services.AddStompDeviceAdapter();
                }

                services.Configure<BrokerDeviceAdapterOptions>(o =>
                {
                    o.Host = cliOptions.Host ?? string.Empty;
                    o.Port = int.TryParse(cliOptions.Port, global::System.Globalization.CultureInfo.InvariantCulture, out var brokerPort) ? brokerPort : 0;
                    o.SubscribeTopics = Topics(cliOptions.Subscribe);
                    o.PublishTopic = cliOptions.Publish;
                    o.Username = cliOptions.Username;
                    o.Password = cliOptions.Password;
                    o.UseTls = cliOptions.Tls;
                    o.TlsCaCertificatePath = cliOptions.CaCertificate;
                    o.TimeoutMs = cliOptions.WriteTimeoutMs;
                });
                break;
            default:
                services.AddSerialDeviceAdapter();
                services.Configure<SerialDeviceAdapterOptions>(o =>
                {
                    o.PortName = cliOptions.Port!;
                    o.BaudRate = cliOptions.Baud;
                    o.DataBits = cliOptions.DataBits;
                    o.Parity = (global::System.IO.Ports.Parity)(int)cliOptions.Parity;
                    o.StopBits = (global::System.IO.Ports.StopBits)(int)cliOptions.StopBits;
                    o.Handshake = cliOptions.Handshake;
                    o.WriteTimeoutMs = cliOptions.WriteTimeoutMs;
                    o.ReadTimeoutMs = cliOptions.ReadTimeoutMs;
                    o.DtrEnable = cliOptions.Dtr;
                    o.RtsEnable = cliOptions.Rts;
                    o.WriteByteDelayMs = cliOptions.WriteByteDelayMs;
                });
                break;
        }

        return services;
    }

    private static List<string> Topics(string? commaSeparated) =>
        [.. (commaSeparated ?? string.Empty).Split(',', global::System.StringSplitOptions.RemoveEmptyEntries | global::System.StringSplitOptions.TrimEntries)];

    /// <summary>Loads plugins from <see cref="CliOptions.Plugins"/> (default: <c>plugins</c> next to the app) and registers their <see cref="PluginLoadResult"/>s.</summary>
    public static IServiceCollection AddPlugins(this IServiceCollection services, CliOptions cliOptions)
    {
        var directory = PluginLoader.ResolveDirectory(cliOptions.Plugins);
        var results = PluginLoader.LoadAll(directory, services);
        if (cliOptions.Transport is { Length: > 0 } selected
            && services.Where(d => d.ServiceType == typeof(PluginTransport)).Select(d => (PluginTransport)d.ImplementationInstance!)
                .FirstOrDefault(t => string.Equals(t.Name, selected, StringComparison.OrdinalIgnoreCase)) is { } chosen)
        {
            services.AddTransient<IDeviceAdapter>(_ => chosen.Create());
        }

        services.AddSingleton<IReadOnlyList<PluginLoadResult>>(results);
        PublishInstrumentProviders(services);
        return services;
    }

    // The Connection Editor lists each instrument provider's profiles but runs outside a built host (and before one, on first
    // run), so the providers the plugins registered are also published to a static. A throwaway container is enough: a provider
    // is stateless and takes no dependencies.
    private static void PublishInstrumentProviders(IServiceCollection services)
    {
        using var container = services.BuildServiceProvider();
        InstrumentPanelProviders.Set(container.GetServices<IInstrumentPanelProvider>());
    }

    // Opt-in (--otlp). Started here rather than as a hosted service because front ends build their host but never start it.
    private static void StartTelemetry(CliOptions cliOptions)
    {
        if (TelemetryExporter.ParseEndpoint(cliOptions.Otlp) is not { } endpoint)
        {
            return;
        }

        var exporter = TelemetryExporter.Start(endpoint, "devterm", DevTermTelemetry.Name);
        AppDomain.CurrentDomain.ProcessExit += (_, _) => exporter.Dispose();
    }

    /// <summary>
    /// The core engine and every presenter, configured from <paramref name="cliOptions"/>, but no transport. What
    /// <see cref="AddDevTermFrontEnd"/> builds on and what playback composes, so replaying a log can't reach a real device.
    /// </summary>
    public static IServiceCollection AddDevTermPresenters(this IServiceCollection services, CliOptions cliOptions)
    {
        ArgumentNullException.ThrowIfNull(cliOptions);
        services.AddDevTermCore();
        services.TryAddTextPresenters();
        services.AddPlugins(cliOptions);
        services.Configure<AsciiPresenterOptions>(o => o.MaxLineLength = cliOptions.AsciiMaxLineLength);
        services.AddStreamCaptureConverter(cliOptions);
        return services;
    }

    /// <summary>Registers <see cref="StreamCaptureConverter"/> and its options, bound from <paramref name="cliOptions"/>'s <c>StreamConvert*</c> properties.</summary>
    public static IServiceCollection AddStreamCaptureConverter(this IServiceCollection services, CliOptions cliOptions)
    {
        ArgumentNullException.ThrowIfNull(cliOptions);
        services.Configure<StreamCaptureConverterOptions>(o => StreamCaptureConverterOptions.CopyFrom(cliOptions, o));
        services.AddSingleton<StreamCaptureConverter>();
        return services;
    }
}
