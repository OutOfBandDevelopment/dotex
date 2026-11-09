using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.Telemetry.Tests;

[TestCategory("Unit")]
[TestClass]
public sealed class TelemetryExporterTests
{
    [TestMethod]
    public void ParseEndpoint_TrueMeansTheLocalCollector()
    {
        Assert.AreEqual(new Uri("http://localhost:4317"), TelemetryExporter.ParseEndpoint("true"));
        Assert.AreEqual(new Uri("https://otel.example:4317"), TelemetryExporter.ParseEndpoint("https://otel.example:4317"));
    }

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("false")]
    [DataRow("localhost:4317")]
    [DataRow("ftp://host/")]
    public void ParseEndpoint_RejectsAnythingThatIsNotAnHttpUrl(string? text)
    {
        Assert.IsNull(TelemetryExporter.ParseEndpoint(text));
    }

    [TestMethod]
    public void Start_AgainstAnUnreachableCollector_DoesNotThrowAndDisposesCleanly()
    {
        using var exporter = TelemetryExporter.Start(new Uri("http://127.0.0.1:1"), "telemetry-test", "Test");

        Assert.IsNotNull(exporter);
    }
}

[TestCategory("Unit")]
[TestClass]
public sealed class OtlpProtocolTests
{
    private const string Variable = "OTEL_EXPORTER_OTLP_PROTOCOL";

    [TestMethod]
    public void Apply_WithNoProtocolAndNoVariable_UsesHttpProtobuf()
    {
        // Stage
        var saved = Environment.GetEnvironmentVariable(Variable);
        Environment.SetEnvironmentVariable(Variable, null);
        try
        {
            var exporter = new OpenTelemetry.Exporter.OtlpExporterOptions();

            // Test
            ServiceCollectionExtensions.Apply(exporter, new OpenTelemetryOptions(), "v1/traces");

            // Assert
            Assert.AreEqual(OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf, exporter.Protocol);
        }
        finally
        {
            Environment.SetEnvironmentVariable(Variable, saved);
        }
    }

    [TestMethod]
    public void Apply_WithNoProtocolAndGrpcVariable_KeepsGrpc()
    {
        // Stage
        var saved = Environment.GetEnvironmentVariable(Variable);
        Environment.SetEnvironmentVariable(Variable, "grpc");
        try
        {
            var exporter = new OpenTelemetry.Exporter.OtlpExporterOptions();

            // Test
            ServiceCollectionExtensions.Apply(exporter, new OpenTelemetryOptions(), "v1/traces");

            // Assert
            Assert.AreEqual(OpenTelemetry.Exporter.OtlpExportProtocol.Grpc, exporter.Protocol);
        }
        finally
        {
            Environment.SetEnvironmentVariable(Variable, saved);
        }
    }

    [TestMethod]
    public void Apply_WithConfiguredProtocol_WinsOverTheVariable()
    {
        // Stage
        var saved = Environment.GetEnvironmentVariable(Variable);
        Environment.SetEnvironmentVariable(Variable, "grpc");
        try
        {
            var exporter = new OpenTelemetry.Exporter.OtlpExporterOptions();

            // Test
            ServiceCollectionExtensions.Apply(exporter, new OpenTelemetryOptions { Protocol = "HttpProtobuf" }, "v1/traces");

            // Assert
            Assert.AreEqual(OpenTelemetry.Exporter.OtlpExportProtocol.HttpProtobuf, exporter.Protocol);
        }
        finally
        {
            Environment.SetEnvironmentVariable(Variable, saved);
        }
    }
}
