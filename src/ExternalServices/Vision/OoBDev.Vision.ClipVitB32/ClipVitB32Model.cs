using Microsoft.Extensions.Logging;
using OoBDev.Onnx.ImageEmbeddings;
using OoBDev.Onnx.SentenceEmbeddings;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Vision.ClipVitB32;

/// <summary>
/// The CLIP ViT-B/32 files (image tower, text tower and tokenizer): a pinned Hugging Face revision with SHA-256
/// hashes, downloaded on first use into the shared hub cache.
/// </summary>
public static class ClipVitB32Model
{
    /// <summary>Hugging Face repository the ONNX export comes from.</summary>
    public const string Repository = "Xenova/clip-vit-base-patch32";

    /// <summary>Pinned revision (commit) of <see cref="Repository"/>.</summary>
    public const string Revision = "d15189d7028b43f1d3e65039190477f6af591c2a";

    /// <summary>Default model folder: the snapshot folder of the Hugging Face hub cache.</summary>
    public static string DefaultFolder { get; } = HuggingFaceHubCache.SnapshotFolder(Repository, Revision);

    private static List<ModelFileSource> VisionFiles() =>
    [
        HuggingFaceHubCache.File(Repository, Revision, "onnx/vision_model.onnx", "fd6e1402a588279d1723c7534d4bcba5bc0b14b47dfab0e46f8c47b8270d7d40"),
    ];

    private static IReadOnlyList<ModelFileSource> TextFiles() =>
    [
        HuggingFaceHubCache.File(Repository, Revision, "onnx/text_model.onnx", "3f6571f5bad13a97c469c1622e1cfc4d9aef78b79fdbfcff804ca357bfada8cc"),
        HuggingFaceHubCache.File(Repository, Revision, "merges.txt", "9fd691f7c8039210e0fced15865466c65820d09b63988b0174bfe25de299051a"),
        HuggingFaceHubCache.File(Repository, Revision, "vocab.json", "5047b556ce86ccaf6aa22b3ffccfc52d391ea4accdab9c2f2407da5b742d4363"),
    ];

    /// <summary>Sets the image tower folder, preprocessing and download sources to the CLIP defaults.</summary>
    /// <param name="options">The options to fill.</param>
    public static void ApplyDefaults(OnnxImageEmbeddingOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        options.ModelPath = DefaultFolder;
        options.ModelFileName = "onnx/vision_model.onnx";
        options.InputName = "pixel_values";
        options.OutputName = "image_embeds";
        options.ImageSize = 224;
        options.ResizeShortestEdge = 224;
        options.CenterCrop = true;
        options.Resample = ImageResample.Bicubic;
        options.Mean = [0.48145466f, 0.4578275f, 0.40821073f];
        options.Std = [0.26862954f, 0.26130258f, 0.27577711f];
        options.Pooling = ImagePooling.Output;
        options.Normalize = true;
        options.ModelFiles = VisionFiles();
    }

    /// <summary>Sets the text tower folder and file names to the CLIP defaults.</summary>
    /// <param name="options">The options to fill.</param>
    public static void ApplyDefaults(ClipTextOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        options.ModelPath = DefaultFolder;
    }

    /// <summary>Downloads the image tower files that are missing.</summary>
    /// <param name="options">Options filled by <see cref="ApplyDefaults(OnnxImageEmbeddingOptions)"/>.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="cancellationToken">Cancellation.</param>
    /// <returns>The model folder.</returns>
    public static Task<string> EnsureAsync(OnnxImageEmbeddingOptions options, ILogger logger, CancellationToken cancellationToken = default) =>
        OnnxImageEmbeddingGenerator.EnsureModelAsync(options, logger, cancellationToken);

    /// <summary>Downloads the text tower and tokenizer files that are missing into the text model folder.</summary>
    /// <param name="options">Text options; the folder is <see cref="ClipTextOptions.ModelPath"/>.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="cancellationToken">Cancellation.</param>
    /// <returns>The model folder.</returns>
    public static async Task<string> EnsureAsync(ClipTextOptions options, ILogger logger, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(options);
        var folder = ClipTextEmbeddingGenerator.ResolveFolder(options);
        await ModelDownloader.EnsureAsync(folder, TextFiles(), logger, cancellationToken).ConfigureAwait(false);
        return folder;
    }
}
