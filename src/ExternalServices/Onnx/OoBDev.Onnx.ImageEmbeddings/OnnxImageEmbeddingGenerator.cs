using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OoBDev.Onnx.SentenceEmbeddings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Onnx.ImageEmbeddings;

/// <summary>
/// Creates image embeddings with an ONNX Runtime session that runs in this process. Inputs are
/// <see cref="DataContent"/> items that hold encoded images. One instance is safe to share between threads.
/// </summary>
public sealed class OnnxImageEmbeddingGenerator : IEmbeddingGenerator<DataContent, Embedding<float>>
{
    private readonly OnnxImageEmbeddingOptions _options;
    private readonly ImageModel _model;
    private readonly EmbeddingGeneratorMetadata _metadata;

    /// <summary>
    /// Loads the model named by the options.
    /// </summary>
    /// <param name="options">Generator settings.</param>
    /// <param name="decoder">Decodes the encoded images.</param>
    /// <param name="logger">Logger.</param>
    public OnnxImageEmbeddingGenerator(
        IOptions<OnnxImageEmbeddingOptions> options,
        IImageDecoder decoder,
        ILogger<OnnxImageEmbeddingGenerator> logger)
    {
        ArgumentNullException.ThrowIfNull(options);
        _options = options.Value;
        _model = new ImageModel(_options, decoder, logger);
        _metadata = new EmbeddingGeneratorMetadata(nameof(OnnxImageEmbeddingGenerator), defaultModelId: _options.ModelFileName, defaultModelDimensions: Dimensions);
    }

    /// <summary>
    /// Downloads the configured <see cref="OnnxImageEmbeddingOptions.ModelFiles"/> that are missing from the model folder.
    /// </summary>
    /// <param name="options">Generator settings.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="cancellationToken">Cancellation.</param>
    /// <returns>The resolved model folder.</returns>
    public static async Task<string> EnsureModelAsync(OnnxImageEmbeddingOptions options, ILogger logger, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);
        options.Validate();
        var folder = ImageModel.ResolveFolder(options);
        await ModelDownloader.EnsureAsync(folder, options.ModelFiles, logger, cancellationToken).ConfigureAwait(false);
        return folder;
    }

    /// <summary>Size of the vectors produced, read from the model.</summary>
    public int Dimensions => _model.Size;

    /// <inheritdoc />
    public async Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(
        IEnumerable<DataContent> values,
        EmbeddingGenerationOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(values);
        var inputs = values as IReadOnlyList<DataContent> ?? [.. values];
        var images = new List<ReadOnlyMemory<byte>>(inputs.Count);
        for (var i = 0; i < inputs.Count; i++)
        {
            var content = inputs[i] ?? throw new ArgumentException($"Item {i} is null.", nameof(values));
            if (content.MediaType is { Length: > 0 } type && !type.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                throw new ArgumentException($"Item {i} has media type '{type}', not an image.", nameof(values));
            images.Add(content.Data);
        }

        var vectors = await _model.RunAsync(images, cancellationToken).ConfigureAwait(false);
        var result = new GeneratedEmbeddings<Embedding<float>>(vectors.Length);
        foreach (var vector in vectors)
        {
            if (_options.Normalize) VectorMath.Normalize(vector);
            result.Add(new Embedding<float>(vector));
        }
        return result;
    }

    /// <inheritdoc />
    public object? GetService(Type serviceType, object? serviceKey = null)
    {
        ArgumentNullException.ThrowIfNull(serviceType);
        if (serviceKey is not null) return null;
        if (serviceType == typeof(EmbeddingGeneratorMetadata)) return _metadata;
        return serviceType.IsInstanceOfType(this) ? this : null;
    }

    /// <inheritdoc />
    public void Dispose() => _model.Dispose();
}
