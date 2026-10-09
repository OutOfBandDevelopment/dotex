using Microsoft.Extensions.AI;
using OoBDev.Onnx.ImageEmbeddings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics.Tensors;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Vision.ClipVitB32;

/// <summary>
/// Classifies an image against labels chosen at call time: the cosine similarity of the image vector and each label's
/// text vector, scaled by the CLIP logit scale and turned into probabilities with softmax.
/// </summary>
public sealed class ZeroShotImageClassifier
{
    /// <summary>The CLIP logit scale (exp of the trained temperature, 100).</summary>
    public const float LogitScale = 100f;

    private readonly IEmbeddingGenerator<DataContent, Embedding<float>> _images;
    private readonly IEmbeddingGenerator<string, Embedding<float>> _texts;

    /// <summary>Creates the classifier from the two CLIP towers.</summary>
    /// <param name="images">The CLIP image embedding generator.</param>
    /// <param name="texts">The CLIP text embedding generator.</param>
    public ZeroShotImageClassifier(IEmbeddingGenerator<DataContent, Embedding<float>> images, IEmbeddingGenerator<string, Embedding<float>> texts)
    {
        _images = images ?? throw new ArgumentNullException(nameof(images));
        _texts = texts ?? throw new ArgumentNullException(nameof(texts));
    }

    /// <summary>Scores the image against the labels.</summary>
    /// <param name="image">The encoded image.</param>
    /// <param name="labels">Candidate labels, for example <c>a photo of a cat</c>.</param>
    /// <param name="topK">Number of results, best first; 0 or more than the labels returns all.</param>
    /// <param name="cancellationToken">Cancellation.</param>
    /// <returns>Labels with probabilities that add up to one over all labels.</returns>
    public async Task<IReadOnlyList<ImageLabel>> ClassifyAsync(DataContent image, IReadOnlyList<string> labels, int topK = 5, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(image);
        ArgumentNullException.ThrowIfNull(labels);
        if (labels.Count == 0) throw new ArgumentException("At least one label is required.", nameof(labels));
        ArgumentOutOfRangeException.ThrowIfNegative(topK);

        var imageVector = (await _images.GenerateAsync([image], cancellationToken: cancellationToken).ConfigureAwait(false))[0].Vector;
        var textVectors = await _texts.GenerateAsync(labels, cancellationToken: cancellationToken).ConfigureAwait(false);

        var logits = new float[labels.Count];
        for (var i = 0; i < logits.Length; i++) logits[i] = LogitScale * TensorPrimitives.CosineSimilarity(imageVector.Span, textVectors[i].Vector.Span);
        // SoftMax does not subtract the maximum, and exp(100) overflows float, so shift the logits first.
        TensorPrimitives.Subtract(logits, TensorPrimitives.Max(logits), logits);
        var probabilities = new float[logits.Length];
        TensorPrimitives.SoftMax(logits, probabilities);

        var ranked = Enumerable.Range(0, labels.Count).OrderByDescending(i => probabilities[i]).Select(i => new ImageLabel(labels[i], probabilities[i]));
        return [.. (topK == 0 ? ranked : ranked.Take(topK))];
    }
}
