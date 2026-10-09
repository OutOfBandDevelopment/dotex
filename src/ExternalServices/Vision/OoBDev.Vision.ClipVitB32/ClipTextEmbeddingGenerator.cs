using Microsoft.Extensions.AI;
using Microsoft.ML.OnnxRuntime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics.Tensors;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Vision.ClipVitB32;

/// <summary>
/// Creates CLIP text embeddings (the same 512 dimension space as the CLIP image embeddings) with an ONNX Runtime
/// session that runs in this process. One instance is safe to share between threads.
/// </summary>
public sealed class ClipTextEmbeddingGenerator : IEmbeddingGenerator<string, Embedding<float>>
{
    private readonly ClipTextOptions _options;
    private readonly ClipTokenizer _tokenizer;
    private readonly InferenceSession _session;
    private readonly SemaphoreSlim _gate;
    private readonly EmbeddingGeneratorMetadata _metadata;

    /// <summary>Loads the text tower and the tokenizer from the model folder.</summary>
    /// <param name="options">Text settings; the files must already be in <see cref="ClipTextOptions.ModelPath"/>.</param>
    public ClipTextEmbeddingGenerator(ClipTextOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        options.Validate();
        _options = options;

        var folder = ResolveFolder(options);
        var modelFile = Path.Combine(folder, options.ModelFileName);
        if (!File.Exists(modelFile)) throw new FileNotFoundException("CLIP text model not found.", modelFile);
        _tokenizer = ClipTokenizer.Load(Path.Combine(folder, options.VocabFileName), Path.Combine(folder, options.MergesFileName));

        _session = new InferenceSession(modelFile);
        if (!_session.OutputMetadata.TryGetValue(options.OutputName, out var metadata)) throw new ArgumentException($"The model has no output '{options.OutputName}'.", nameof(options));
        Dimensions = metadata.Dimensions[^1];
        _gate = new SemaphoreSlim(options.MaxConcurrentInferences);
        _metadata = new EmbeddingGeneratorMetadata(nameof(ClipTextEmbeddingGenerator), defaultModelId: options.ModelFileName, defaultModelDimensions: Dimensions);
    }

    /// <summary>Size of the vectors produced, read from the model.</summary>
    public int Dimensions { get; }

    internal static string ResolveFolder(ClipTextOptions options) =>
        Path.IsPathRooted(options.ModelPath) ? options.ModelPath : Path.Combine(AppContext.BaseDirectory, options.ModelPath);

    /// <inheritdoc />
    public async Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(
        IEnumerable<string> values,
        EmbeddingGenerationOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(values);
        var texts = values as IReadOnlyList<string> ?? [.. values];
        var result = new GeneratedEmbeddings<Embedding<float>>(texts.Count);
        var vectors = new float[texts.Count][];

        foreach (var batch in Enumerable.Range(0, texts.Count).Chunk(_options.MaxBatchSize))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var encoded = batch.Select(i => _tokenizer.Encode(texts[i] ?? throw new ArgumentException($"Item {i} is null.", nameof(values)), _options.MaxSequenceLength)).ToArray();
            var length = encoded.Max(e => e.Length);
            var ids = new long[batch.Length * length];
            Array.Fill(ids, ClipTokenizer.EndOfText);
            for (var b = 0; b < batch.Length; b++)
                for (var t = 0; t < encoded[b].Length; t++) ids[(b * length) + t] = encoded[b][t];

            await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                var rows = await Task.Run(() => Infer(ids, batch.Length, length), cancellationToken).ConfigureAwait(false);
                for (var b = 0; b < batch.Length; b++) vectors[batch[b]] = rows[b];
            }
            finally
            {
                _gate.Release();
            }
        }

        foreach (var vector in vectors)
        {
            if (_options.Normalize)
            {
                var norm = TensorPrimitives.Norm(vector);
                if (norm > 0f) TensorPrimitives.Divide(vector, norm, vector);
            }
            result.Add(new Embedding<float>(vector));
        }
        return result;
    }

    private float[][] Infer(long[] ids, int count, int length)
    {
        using var input = OrtValue.CreateTensorValueFromMemory(ids, [count, length]);
        using var runOptions = new RunOptions();
        using var output = _session.Run(runOptions, [_options.InputName], [input], [_options.OutputName]);
        var data = output[0].GetTensorDataAsSpan<float>();
        var rows = new float[count][];
        for (var b = 0; b < count; b++) rows[b] = data.Slice(b * Dimensions, Dimensions).ToArray();
        return rows;
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
    public void Dispose()
    {
        _session.Dispose();
        _gate.Dispose();
    }
}
