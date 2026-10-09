using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.ML.OnnxRuntime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Onnx.SentenceEmbeddings;

/// <summary>
/// Creates sentence embeddings with an ONNX Runtime session that runs in this process.
/// One instance is safe to share between threads: the session and tokenizer are shared and
/// read-only, every call allocates its own buffers, and a semaphore bounds concurrent runs.
/// </summary>
public sealed partial class OnnxSentenceEmbeddingGenerator : IEmbeddingGenerator<string, Embedding<float>>
{
    private readonly OnnxSentenceEmbeddingOptions _options;
    private readonly ILogger<OnnxSentenceEmbeddingGenerator> _logger;
    private readonly SentenceTokenizer _tokenizer;
    private readonly InferenceSession _session;
    private readonly SemaphoreSlim _gate;
    private readonly string _outputName;
    private readonly int _hidden;
    private readonly EmbeddingGeneratorMetadata _metadata;

    /// <summary>
    /// Loads the model and vocabulary named by the options.
    /// </summary>
    /// <param name="options">Generator settings.</param>
    /// <param name="logger">Logger.</param>
    public OnnxSentenceEmbeddingGenerator(
        IOptions<OnnxSentenceEmbeddingOptions> options,
        ILogger<OnnxSentenceEmbeddingGenerator> logger)
    {
        _options = options.Value;
        _logger = logger;
        _options.Validate();

        var folder = ResolveFolder(_options);
        if (_options.ModelFiles.Count > 0) ModelDownloader.EnsureAsync(folder, _options.ModelFiles, _logger, CancellationToken.None).GetAwaiter().GetResult();
        var modelFile = Path.Combine(folder, _options.ModelFileName);
        var vocabFile = Path.Combine(folder, _options.VocabFileName);
        if (!File.Exists(modelFile)) throw new FileNotFoundException("Embedding model not found.", modelFile);
        if (!File.Exists(vocabFile)) throw new FileNotFoundException("Embedding vocabulary not found.", vocabFile);

        _tokenizer = new SentenceTokenizer(vocabFile, _options.LowerCase, _options.MaxSequenceLength);

        using var sessionOptions = new SessionOptions();
        if (_options.IntraOpThreads > 0) sessionOptions.IntraOpNumThreads = _options.IntraOpThreads;
        _session = new InferenceSession(modelFile, sessionOptions);

        _outputName = _session.OutputNames[0];
        _hidden = _session.OutputMetadata[_outputName].Dimensions[^1];
        if (_hidden < 1) throw new InvalidOperationException($"Output '{_outputName}' does not report a fixed vector size.");
        if (_options.Dimensions is { } d && d > _hidden)
            throw new ArgumentException($"Dimensions ({d}) is larger than the model output ({_hidden}).", nameof(options));

        _gate = new SemaphoreSlim(_options.MaxConcurrentInferences);
        Dimensions = _options.Dimensions ?? _hidden;
        _metadata = new EmbeddingGeneratorMetadata(nameof(OnnxSentenceEmbeddingGenerator), defaultModelId: _options.ModelFileName, defaultModelDimensions: Dimensions);
        LogLoaded(modelFile, Dimensions);
    }

    /// <summary>
    /// Downloads the configured <see cref="OnnxSentenceEmbeddingOptions.ModelFiles"/> that are missing from the model folder.
    /// Call at startup to warm the folder instead of paying for the download on first use.
    /// </summary>
    /// <param name="options">Generator settings.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="cancellationToken">Cancellation.</param>
    /// <returns>The resolved model folder.</returns>
    public static async Task<string> EnsureModelAsync(OnnxSentenceEmbeddingOptions options, ILogger logger, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);
        options.Validate();
        var folder = ResolveFolder(options);
        await ModelDownloader.EnsureAsync(folder, options.ModelFiles, logger, cancellationToken).ConfigureAwait(false);
        return folder;
    }

    private static string ResolveFolder(OnnxSentenceEmbeddingOptions options) =>
        Path.IsPathRooted(options.ModelPath) ? options.ModelPath : Path.Combine(AppContext.BaseDirectory, options.ModelPath);

    /// <summary>Size of the vectors produced, read from the model.</summary>
    public int Dimensions { get; }

    /// <inheritdoc />
    public async Task<GeneratedEmbeddings<Embedding<float>>> GenerateAsync(
        IEnumerable<string> values,
        EmbeddingGenerationOptions? options = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(values);
        var inputs = values as IReadOnlyList<string> ?? [.. values];
        var vectors = new float[inputs.Count][];

        // Tokenize once per value; blank input gets a zero vector without running the model.
        var work = new List<(int Index, IReadOnlyList<int> Ids)>(inputs.Count);
        for (var i = 0; i < inputs.Count; i++)
        {
            var text = inputs[i];
            if (string.IsNullOrWhiteSpace(text))
            {
                vectors[i] = new float[Dimensions];
                continue;
            }
            work.Add((i, _tokenizer.Encode(_options.Prefix is null ? text : _options.Prefix + text)));
        }

        foreach (var batch in work.Chunk(_options.MaxBatchSize))
        {
            cancellationToken.ThrowIfCancellationRequested();
            await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                var rows = await Task.Run(() => RunBatch(batch), cancellationToken).ConfigureAwait(false);
                for (var b = 0; b < batch.Length; b++) vectors[batch[b].Index] = rows[b];
            }
            finally
            {
                _gate.Release();
            }
        }

        var result = new GeneratedEmbeddings<Embedding<float>>(inputs.Count);
        foreach (var vector in vectors) result.Add(new Embedding<float>(vector));
        return result;
    }

    private float[][] RunBatch((int Index, IReadOnlyList<int> Ids)[] batch)
    {
        var longest = batch.Max(item => item.Ids.Count);
        var size = batch.Length * longest;
        var ids = new long[size];
        var mask = new long[size];
        var types = new long[size];
        for (var b = 0; b < batch.Length; b++)
        {
            var row = batch[b].Ids;
            for (var t = 0; t < row.Count; t++)
            {
                ids[(b * longest) + t] = row[t];
                mask[(b * longest) + t] = 1;
            }
        }

        var shape = new long[] { batch.Length, longest };
        var names = new List<string> { _options.InputIdsName, _options.AttentionMaskName };
        using var idsValue = OrtValue.CreateTensorValueFromMemory(ids, shape);
        using var maskValue = OrtValue.CreateTensorValueFromMemory(mask, shape);
        var values = new List<OrtValue> { idsValue, maskValue };
        OrtValue? typesValue = null;
        try
        {
            if (_options.TokenTypeIdsName is { } typeName)
            {
                typesValue = OrtValue.CreateTensorValueFromMemory(types, shape);
                names.Add(typeName);
                values.Add(typesValue);
            }

            using var runOptions = new RunOptions();
            using var output = _session.Run(runOptions, names, values, [_outputName]);
            var hiddenStates = output[0].GetTensorDataAsSpan<float>();

            var rows = new float[batch.Length][];
            for (var b = 0; b < batch.Length; b++)
            {
                var pooled = new float[_hidden];
                EmbeddingMath.Pool(
                    hiddenStates.Slice(b * longest * _hidden, longest * _hidden),
                    mask.AsSpan(b * longest, longest),
                    _hidden,
                    _options.Pooling,
                    pooled);
                if (Dimensions < _hidden) pooled = pooled[..Dimensions];
                if (_options.Normalize || Dimensions < _hidden) EmbeddingMath.Normalize(pooled);
                rows[b] = pooled;
            }
            return rows;
        }
        finally
        {
            typesValue?.Dispose();
        }
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

    [LoggerMessage(Level = LogLevel.Information, Message = "Loaded embedding model {ModelFile} ({Dimensions} dimensions)")]
    private partial void LogLoaded(string modelFile, int dimensions);
}
