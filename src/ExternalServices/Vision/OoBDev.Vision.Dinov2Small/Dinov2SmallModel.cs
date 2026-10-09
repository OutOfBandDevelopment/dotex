using Microsoft.Extensions.Logging;
using OoBDev.Onnx.ImageEmbeddings;
using OoBDev.Onnx.SentenceEmbeddings;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Vision.Dinov2Small;

/// <summary>
/// The DINOv2-small model files: a pinned Hugging Face revision with a SHA-256 hash, downloaded on first use.
/// </summary>
public static class Dinov2SmallModel
{
    /// <summary>Hugging Face repository the ONNX export comes from.</summary>
    public const string Repository = "Xenova/dinov2-small";

    /// <summary>Pinned revision (commit) of <see cref="Repository"/>.</summary>
    public const string Revision = "c2bb04a51fab207c420665f1946016107bffc701";

    private const string ModelSha256 = "83141175ec78b4ff9a2bb58a4c7c264ba0054d1c2e122e5a8114b79a8d4179ea";

    /// <summary>
    /// Default model folder: the snapshot folder of the Hugging Face hub cache, shared with Python tools and other apps.
    /// </summary>
    public static string DefaultFolder { get; } = HuggingFaceHubCache.SnapshotFolder(Repository, Revision);

    /// <summary>Sets the folder, preprocessing and download sources to the defaults of this model.</summary>
    /// <param name="options">The options to fill.</param>
    public static void ApplyDefaults(OnnxImageEmbeddingOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        options.ModelPath = DefaultFolder;
        options.ModelFileName = "onnx/model.onnx";
        options.InputName = "pixel_values";
        options.OutputName = "last_hidden_state";
        options.ImageSize = 224;
        options.ResizeShortestEdge = 256;
        options.CenterCrop = true;
        options.Resample = ImageResample.Bicubic;
        options.Mean = [0.485f, 0.456f, 0.406f];
        options.Std = [0.229f, 0.224f, 0.225f];
        options.Pooling = ImagePooling.FirstToken;
        options.Normalize = true;
        options.ModelFiles = [HuggingFaceHubCache.File(Repository, Revision, "onnx/model.onnx", ModelSha256)];
    }

    /// <summary>Downloads the model files that are missing, for example at startup.</summary>
    /// <param name="options">Options filled by <see cref="ApplyDefaults"/> and the configuration.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="cancellationToken">Cancellation.</param>
    /// <returns>The model folder.</returns>
    public static Task<string> EnsureAsync(OnnxImageEmbeddingOptions options, ILogger logger, CancellationToken cancellationToken = default) =>
        OnnxImageEmbeddingGenerator.EnsureModelAsync(options, logger, cancellationToken);
}
