using Microsoft.Extensions.Logging;
using OoBDev.Onnx.ImageEmbeddings;
using OoBDev.Onnx.SentenceEmbeddings;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.Vision.VitBasePatch16;

/// <summary>
/// The ViT-base-patch16-224 ImageNet classifier files: a pinned Hugging Face revision with SHA-256 hashes,
/// downloaded on first use.
/// </summary>
public static class VitBasePatch16Model
{
    /// <summary>Hugging Face repository the ONNX export comes from.</summary>
    public const string Repository = "Xenova/vit-base-patch16-224";

    /// <summary>Pinned revision (commit) of <see cref="Repository"/>.</summary>
    public const string Revision = "66fef688e8dbe77dd9d5aa256353f9ad8b0ef799";

    private const string ModelSha256 = "4bafe23c7e2650856449a792eafcc1d3bab4a2f41bcf58c9f3eac99d98719fcc";
    private const string ConfigSha256 = "ba68592930f3a1aa36c96630d374018d02c09fa59a7e38a3978613cadba02af4";

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
        options.OutputName = "logits";
        options.ImageSize = 224;
        options.ResizeShortestEdge = null;
        options.CenterCrop = false;
        options.Resample = ImageResample.Bilinear;
        options.Mean = [0.5f, 0.5f, 0.5f];
        options.Std = [0.5f, 0.5f, 0.5f];
        options.Pooling = ImagePooling.Output;
        options.Normalize = false;
        options.ModelFiles =
        [
            HuggingFaceHubCache.File(Repository, Revision, "onnx/model.onnx", ModelSha256),
            HuggingFaceHubCache.File(Repository, Revision, "config.json", ConfigSha256),
        ];
    }

    /// <summary>Downloads the model files that are missing, for example at startup.</summary>
    /// <param name="options">Options filled by <see cref="ApplyDefaults"/> and the configuration.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="cancellationToken">Cancellation.</param>
    /// <returns>The model folder.</returns>
    public static Task<string> EnsureAsync(OnnxImageEmbeddingOptions options, ILogger logger, CancellationToken cancellationToken = default) =>
        OnnxImageEmbeddingGenerator.EnsureModelAsync(options, logger, cancellationToken);

    /// <summary>Reads the class labels (<c>id2label</c>) from <c>config.json</c> in the model folder, in class order.</summary>
    /// <param name="modelFolder">The folder that holds <c>config.json</c>.</param>
    /// <returns>One label per class.</returns>
    public static IReadOnlyList<string> LoadLabels(string modelFolder)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(modelFolder);
        using var stream = File.OpenRead(Path.Combine(modelFolder, "config.json"));
        using var document = JsonDocument.Parse(stream);
        var map = document.RootElement.GetProperty("id2label");
        var labels = new string[map.EnumerateObject().Count()];
        foreach (var entry in map.EnumerateObject())
            labels[int.Parse(entry.Name, System.Globalization.CultureInfo.InvariantCulture)] = entry.Value.GetString() ?? string.Empty;
        return labels;
    }
}
