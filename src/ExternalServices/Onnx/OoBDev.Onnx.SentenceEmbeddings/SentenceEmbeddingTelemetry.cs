using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace OoBDev.Onnx.SentenceEmbeddings;

/// <summary>
/// Telemetry names for the ONNX sentence embedding generator. Add <see cref="Name"/> as a trace source and a meter.
/// </summary>
public static class SentenceEmbeddingTelemetry
{
    /// <summary>The <see cref="ActivitySource"/> and <see cref="Meter"/> name.</summary>
    public const string Name = "OoBDev.Onnx.SentenceEmbeddings";

    internal static readonly ActivitySource Source = new(Name);

    private static readonly Meter Meter = new(Name);

    internal static readonly Histogram<double> Duration = Meter.CreateHistogram<double>("oobdev.embeddings.duration", "ms", "Inference time per batch");

    internal static readonly Histogram<int> BatchSize = Meter.CreateHistogram<int>("oobdev.embeddings.batch_size", "{input}", "Inputs per batch");

    internal static readonly Counter<long> Tokens = Meter.CreateCounter<long>("oobdev.embeddings.tokens", "{token}", "Tokens processed");
}
