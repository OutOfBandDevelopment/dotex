using System;
using OpenTelemetry;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace OoBDev.Telemetry;

/// <summary>Sends the named activity sources' traces and meters' metrics to an OTLP collector (gRPC) until disposed.</summary>
public sealed class TelemetryExporter : IDisposable
{
    private readonly TracerProvider _tracer;
    private readonly MeterProvider _meter;

    private TelemetryExporter(TracerProvider tracer, MeterProvider meter)
    {
        _tracer = tracer;
        _meter = meter;
    }

    /// <summary>Parses an OTLP endpoint (<c>true</c> means the usual local collector, <c>http://localhost:4317</c>); null when it isn't an absolute http(s) URL.</summary>
    public static Uri? ParseEndpoint(string? text)
    {
        if (string.Equals(text, "true", StringComparison.OrdinalIgnoreCase))
        {
            return new Uri("http://localhost:4317");
        }

        return Uri.TryCreate(text, UriKind.Absolute, out var uri) && uri.Scheme is "http" or "https" ? uri : null;
    }

    /// <summary>Starts exporting to <paramref name="endpoint"/>. Exporting is asynchronous and best effort: an unreachable collector never throws here or slows the app.</summary>
    /// <param name="sourceNames">The activity source and meter names to export.</param>
    public static TelemetryExporter Start(Uri endpoint, string serviceName, params string[] sourceNames)
    {
        ArgumentNullException.ThrowIfNull(endpoint);
        ArgumentNullException.ThrowIfNull(sourceNames);
        var resource = ResourceBuilder.CreateDefault().AddService(serviceName);

        var tracer = Sdk.CreateTracerProviderBuilder()
            .SetResourceBuilder(resource)
            .AddSource(sourceNames)
            .AddOtlpExporter(o =>
            {
                o.Endpoint = endpoint;
                o.Protocol = OtlpExportProtocol.Grpc;
            })
            .Build();
        var meter = Sdk.CreateMeterProviderBuilder()
            .SetResourceBuilder(resource)
            .AddMeter(sourceNames)
            .AddOtlpExporter((o, reader) =>
            {
                o.Endpoint = endpoint;
                o.Protocol = OtlpExportProtocol.Grpc;
                // The default is 60 s, which makes a short interactive session look like it exported nothing.
                reader.PeriodicExportingMetricReaderOptions.ExportIntervalMilliseconds = 10_000;
            })
            .Build();
        return new TelemetryExporter(tracer, meter);
    }

    public void Dispose()
    {
        _tracer.Dispose();
        _meter.Dispose();
    }
}
