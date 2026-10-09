using Microsoft.Extensions.Logging;
using Microsoft.ML.OnnxRuntime;
using OoBDev.Onnx.SentenceEmbeddings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Onnx.ImageEmbeddings;

/// <summary>
/// The shared core: loads the model, decodes and preprocesses images, runs batches under a concurrency limit and
/// pools the output to one raw vector per image. Thread-safe; every call allocates its own buffers.
/// </summary>
internal sealed partial class ImageModel : IDisposable
{
    private readonly OnnxImageEmbeddingOptions _options;
    private readonly IImageDecoder _decoder;
    private readonly ILogger _logger;
    private readonly ImagePreprocessor _preprocessor;
    private readonly InferenceSession _session;
    private readonly SemaphoreSlim _gate;
    private readonly string _outputName;

    public ImageModel(OnnxImageEmbeddingOptions options, IImageDecoder decoder, ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(decoder);
        options.Validate();
        _options = options;
        _decoder = decoder;
        _logger = logger;
        _preprocessor = new ImagePreprocessor(options);

        var folder = ResolveFolder(options);
        if (options.ModelFiles.Count > 0) ModelDownloader.EnsureAsync(folder, options.ModelFiles, logger, CancellationToken.None).GetAwaiter().GetResult();
        var modelFile = Path.Combine(folder, options.ModelFileName);
        if (!File.Exists(modelFile)) throw new FileNotFoundException("Image model not found.", modelFile);

        using var sessionOptions = new SessionOptions();
        if (options.IntraOpThreads > 0) sessionOptions.IntraOpNumThreads = options.IntraOpThreads;
        _session = new InferenceSession(modelFile, sessionOptions);

        _outputName = options.OutputName ?? _session.OutputNames[0];
        if (!_session.OutputMetadata.TryGetValue(_outputName, out var metadata)) throw new ArgumentException($"The model has no output '{_outputName}'.", nameof(options));
        Size = metadata.Dimensions[^1];
        if (Size < 1) throw new InvalidOperationException($"Output '{_outputName}' does not report a fixed vector size.");
        var rank = metadata.Dimensions.Length;
        if (options.Pooling == ImagePooling.Output ? rank != 2 : rank != 3)
            throw new ArgumentException($"Pooling {options.Pooling} does not fit the output of rank {rank}.", nameof(options));

        _gate = new SemaphoreSlim(options.MaxConcurrentInferences);
        LogLoaded(modelFile, Size);
    }

    /// <summary>Length of the vectors produced.</summary>
    public int Size { get; }

    public static string ResolveFolder(OnnxImageEmbeddingOptions options) =>
        Path.IsPathRooted(options.ModelPath) ? options.ModelPath : Path.Combine(AppContext.BaseDirectory, options.ModelPath);

    public async Task<float[][]> RunAsync(IReadOnlyList<ReadOnlyMemory<byte>> images, CancellationToken cancellationToken)
    {
        var vectors = new float[images.Count][];
        foreach (var batch in Enumerable.Range(0, images.Count).Chunk(_options.MaxBatchSize))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var pixels = new float[batch.Length * _preprocessor.Length];
            for (var b = 0; b < batch.Length; b++)
            {
                var encoded = images[batch[b]];
                if (encoded.Length == 0) throw new ArgumentException($"Image {batch[b]} has no data.", nameof(images));
                if (encoded.Length > _options.MaxImageBytes) throw new ArgumentException($"Image {batch[b]} is larger than MaxImageBytes ({_options.MaxImageBytes}).", nameof(images));
                var decoded = _decoder.Decode(encoded.Span, _options.MaxPixels);
                _preprocessor.Process(decoded, pixels.AsSpan(b * _preprocessor.Length, _preprocessor.Length));
            }

            await _gate.WaitAsync(cancellationToken).ConfigureAwait(false);
            try
            {
                var rows = await Task.Run(() => Infer(pixels, batch.Length), cancellationToken).ConfigureAwait(false);
                for (var b = 0; b < batch.Length; b++) vectors[batch[b]] = rows[b];
            }
            finally
            {
                _gate.Release();
            }
        }
        return vectors;
    }

    private float[][] Infer(float[] pixels, int count)
    {
        var shape = new long[] { count, 3, _options.ImageSize, _options.ImageSize };
        using var input = OrtValue.CreateTensorValueFromMemory(pixels, shape);
        using var runOptions = new RunOptions();
        using var output = _session.Run(runOptions, [_options.InputName], [input], [_outputName]);
        var data = output[0].GetTensorDataAsSpan<float>();

        var rows = new float[count][];
        for (var b = 0; b < count; b++)
        {
            var row = new float[Size];
            switch (_options.Pooling)
            {
                case ImagePooling.Output:
                    data.Slice(b * Size, Size).CopyTo(row);
                    break;
                case ImagePooling.FirstToken:
                {
                    var tokens = data.Length / count / Size;
                    data.Slice(b * tokens * Size, Size).CopyTo(row);
                    break;
                }
                default:
                {
                    var tokens = data.Length / count / Size;
                    if (tokens < 2) throw new InvalidOperationException("The model returned no patch tokens.");
                    var item = data.Slice(b * tokens * Size, tokens * Size);
                    for (var t = 1; t < tokens; t++)
                        for (var d = 0; d < Size; d++) row[d] += item[(t * Size) + d];
                    for (var d = 0; d < Size; d++) row[d] /= tokens - 1;
                    break;
                }
            }
            rows[b] = row;
        }
        return rows;
    }

    public void Dispose()
    {
        _session.Dispose();
        _gate.Dispose();
    }

    [LoggerMessage(Level = LogLevel.Information, Message = "Loaded image model {ModelFile} with vectors of {Size}")]
    private partial void LogLoaded(string modelFile, int size);
}
