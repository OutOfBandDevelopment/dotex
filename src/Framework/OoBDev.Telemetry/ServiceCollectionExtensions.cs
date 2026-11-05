using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OpenTelemetry.Exporter;
using OpenTelemetry.Logs;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System;
using System.Linq;

namespace OoBDev.Telemetry;

/// <summary>
/// Extension methods for configuring OpenTelemetry services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// The environment variable the OTLP exporter reads its endpoint from.
    /// </summary>
    public const string OtlpEndpointVariable = "OTEL_EXPORTER_OTLP_ENDPOINT";

    /// <summary>
    /// Tries to add the OpenTelemetry tracer and logger providers with the OoBDev processors (correlation and user
    /// information for spans and logs) and, when an endpoint is configured, the OTLP exporters.
    /// </summary>
    /// <remarks>
    /// Nothing is added unless the configuration section exists or <c>OTEL_EXPORTER_OTLP_ENDPOINT</c> is set, so hosts
    /// without telemetry settings are unchanged. The processors need <c>IAccessor&lt;CorrelationInfo&gt;</c> and
    /// <c>ICurrentUserAccessor</c> in the container (the web host registers one over <c>IHttpContextAccessor</c>, command line and desktop applications use the Windows session).
    /// </remarks>
    /// <param name="services">The service collection to add the providers to.</param>
    /// <param name="configuration">The configuration containing the options section.</param>
    /// <param name="sectionName">The configuration section name. Default is <c>OpenTelemetryOptions</c>.</param>
    /// <returns>The modified service collection.</returns>
    public static IServiceCollection TryAddOpenTelemetryExtensions(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName = nameof(OpenTelemetryOptions))
    {
        var section = configuration.GetSection(sectionName);
        var options = section.Get<OpenTelemetryOptions>() ?? new();
        var environmentEndpoint = Environment.GetEnvironmentVariable(OtlpEndpointVariable);
        if (!section.Exists() && string.IsNullOrWhiteSpace(environmentEndpoint))
            return services;

        var exportOtlp = options.OtlpEndpoint is not null || !string.IsNullOrWhiteSpace(environmentEndpoint);
        return services.TryAddOpenTelemetryExtensions(options, exportOtlp);
    }

    /// <summary>
    /// Tries to add the OpenTelemetry tracer and logger providers with the OoBDev processors.
    /// </summary>
    /// <param name="services">The service collection to add the providers to.</param>
    /// <param name="options">The telemetry options.</param>
    /// <param name="exportOtlp">Whether to add the OTLP exporters.</param>
    /// <returns>The modified service collection.</returns>
    public static IServiceCollection TryAddOpenTelemetryExtensions(
        this IServiceCollection services,
        OpenTelemetryOptions options,
        bool exportOtlp)
    {
        if (services.Any(d => d.ServiceType == typeof(CorrelationInfoTelemetryProcessor)))
            return services;

        services.TryAddSingleton<CorrelationInfoTelemetryProcessor>();
        services.TryAddSingleton<UserTelemetryProcessor>();
        services.TryAddSingleton<CorrelationInfoLogProcessor>();
        services.TryAddSingleton<UserLogProcessor>();

        var otel = services.AddOpenTelemetry();
        if (!string.IsNullOrWhiteSpace(options.ServiceName))
            otel.ConfigureResource(r => r.AddService(options.ServiceName));

        otel.WithTracing(tracing =>
        {
            tracing.AddSource(options.Sources);
            tracing.AddProcessor(sp => sp.GetRequiredService<CorrelationInfoTelemetryProcessor>());
            tracing.AddProcessor(sp => sp.GetRequiredService<UserTelemetryProcessor>());
            if (exportOtlp)
                tracing.AddOtlpExporter(o => Apply(o, options, "v1/traces"));
        });
        otel.WithLogging(logging =>
        {
            logging.AddProcessor(sp => sp.GetRequiredService<CorrelationInfoLogProcessor>());
            logging.AddProcessor(sp => sp.GetRequiredService<UserLogProcessor>());
            if (exportOtlp)
                logging.AddOtlpExporter(o => Apply(o, options, "v1/logs"));
        });
        return services;
    }

    // An endpoint set in code is used as given, so the HTTP signal path has to be added here (the environment
    // variable gets it appended by the exporter).
    internal static void Apply(OtlpExporterOptions exporter, OpenTelemetryOptions options, string httpSignalPath)
    {
        // Setting the protocol in code overrides OTEL_EXPORTER_OTLP_PROTOCOL, which made an Aspire gRPC endpoint receive HTTP.
        var fromEnvironment = !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_PROTOCOL"));
        var configured = Enum.TryParse<OtlpExportProtocol>(options.Protocol, ignoreCase: true, out var parsed);
        if (configured || !fromEnvironment)
            exporter.Protocol = configured ? parsed : OtlpExportProtocol.HttpProtobuf;
        var protocol = exporter.Protocol;
        if (options.OtlpEndpoint is null)
            return;

        exporter.Endpoint = protocol == OtlpExportProtocol.HttpProtobuf
            ? new Uri(options.OtlpEndpoint.AbsoluteUri.TrimEnd('/') + "/" + httpSignalPath)
            : options.OtlpEndpoint;
    }
}
