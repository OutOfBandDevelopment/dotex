using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Onnx.ImageEmbeddings;

/// <summary>
/// Classifies images with an ONNX classifier model (logits over <see cref="Labels"/>). The options use
/// <see cref="ImagePooling.Output"/> and <see cref="OnnxImageEmbeddingOptions.Normalize"/> is ignored.
/// </summary>
public sealed class OnnxImageClassifier : IImageClassifier, IDisposable
{
    private readonly ImageModel _model;
    private readonly IReadOnlyList<string> _labels;

    /// <summary>Loads the model.</summary>
    /// <param name="options">Model settings.</param>
    /// <param name="labels">One label per logit, in model order.</param>
    /// <param name="decoder">Decodes the encoded images.</param>
    /// <param name="logger">Logger.</param>
    public OnnxImageClassifier(IOptions<OnnxImageEmbeddingOptions> options, IReadOnlyList<string> labels, IImageDecoder decoder, ILogger<OnnxImageClassifier> logger)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(labels);
        _model = new ImageModel(options.Value, decoder, logger);
        if (labels.Count != _model.Size) throw new ArgumentException($"The model has {_model.Size} outputs but {labels.Count} labels were given.", nameof(labels));
        _labels = labels;
    }

    /// <summary>All labels in model order.</summary>
    public IReadOnlyList<string> Labels => _labels;

    /// <inheritdoc />
    public async Task<IReadOnlyList<ImageLabel>> ClassifyAsync(DataContent image, int topK = 5, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(image);
        ArgumentOutOfRangeException.ThrowIfLessThan(topK, 1);
        if (image.MediaType is { Length: > 0 } type && !type.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            throw new ArgumentException($"Media type '{type}' is not an image.", nameof(image));

        var vectors = await _model.RunAsync([image.Data], cancellationToken).ConfigureAwait(false);
        var scores = vectors[0];
        VectorMath.Softmax(scores);
        return [.. Enumerable.Range(0, scores.Length)
            .OrderByDescending(i => scores[i])
            .Take(topK)
            .Select(i => new ImageLabel(_labels[i], scores[i]))];
    }

    /// <inheritdoc />
    public void Dispose() => _model.Dispose();
}
