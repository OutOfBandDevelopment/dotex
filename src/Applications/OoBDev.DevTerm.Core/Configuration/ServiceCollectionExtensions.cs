using System;
using System.Collections.Generic;
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
    /// <exception cref="NotSupportedException">The selected transport has not been ported to the pipe model yet.</exception>
    public static IServiceCollection AddDevTermFrontEnd(this IServiceCollection services, CliOptions cliOptions)
    {
        services.AddDevTermPresenters(cliOptions);
        StartTelemetry(cliOptions);

        if (string.Equals(cliOptions.Transport, "loopback", StringComparison.OrdinalIgnoreCase))
        {
            services.TryAddLoopbackTransport();
            services.Configure<LoopbackTransportOptions>(o => o.SampleIntervalMs = cliOptions.LoopbackSampleIntervalMs);
            return services;
        }

        throw new NotSupportedException($"The '{cliOptions.Transport}' transport is not available yet.");
    }

    /// <summary>Loads plugins from <see cref="CliOptions.Plugins"/> (default: <c>plugins</c> next to the app) and registers their <see cref="PluginLoadResult"/>s.</summary>
    public static IServiceCollection AddPlugins(this IServiceCollection services, CliOptions cliOptions)
    {
        var directory = string.IsNullOrWhiteSpace(cliOptions.Plugins) ? global::System.IO.Path.Combine(AppContext.BaseDirectory, "plugins") : cliOptions.Plugins;
        var results = PluginLoader.LoadAll(directory, services);
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
