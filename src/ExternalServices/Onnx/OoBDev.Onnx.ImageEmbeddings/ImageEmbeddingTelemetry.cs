using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace OoBDev.Onnx.ImageEmbeddings;

/// <summary>
/// Telemetry names for the ONNX image model runner. Add <see cref="Name"/> as a trace source and a meter.
/// </summary>
public static class ImageEmbeddingTelemetry
{
    /// <summary>The <see cref="ActivitySource"/> and <see cref="Meter"/> name.</summary>
    public const string Name = "OoBDev.Onnx.ImageEmbeddings";

    internal static readonly ActivitySource Source = new(Name);

    private static readonly Meter Meter = new(Name);

    internal static readonly Histogram<double> Duration = Meter.CreateHistogram<double>("oobdev.images.duration", "ms", "Inference time per batch");

    internal static readonly Histogram<int> BatchSize = Meter.CreateHistogram<int>("oobdev.images.batch_size", "{image}", "Images per batch");
}
