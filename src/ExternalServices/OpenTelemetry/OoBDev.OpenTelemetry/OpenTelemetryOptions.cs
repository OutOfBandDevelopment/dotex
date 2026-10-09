using System;

namespace OoBDev.OpenTelemetry;

/// <summary>
/// Settings for exporting traces and logs over OTLP.
/// </summary>
/// <remarks>
/// The standard <c>OTEL_*</c> environment variables (for example <c>OTEL_EXPORTER_OTLP_ENDPOINT</c>,
/// <c>OTEL_EXPORTER_OTLP_PROTOCOL</c> and <c>OTEL_EXPORTER_OTLP_HEADERS</c>) are honored by the exporter itself.
/// </remarks>
public class OpenTelemetryOptions
{
    /// <summary>
    /// Gets or sets the OTLP endpoint. When empty the <c>OTEL_EXPORTER_OTLP_ENDPOINT</c> environment variable is used;
    /// when neither is set no exporter is added.
    /// </summary>
    public Uri? OtlpEndpoint { get; set; }

    /// <summary>
    /// Gets or sets the OTLP protocol (<c>Grpc</c> or <c>HttpProtobuf</c>). Default is <c>HttpProtobuf</c>.
    /// </summary>
    public string Protocol { get; set; } = "HttpProtobuf";

    /// <summary>
    /// Gets or sets the <c>service.name</c> resource attribute. When empty the exporter default is kept.
    /// </summary>
    public string? ServiceName { get; set; }

    /// <summary>
    /// Gets or sets the names of the <see cref="global::System.Diagnostics.ActivitySource"/> instances to collect spans from.
    /// </summary>
    public string[] Sources { get; set; } = [];
}
