using OoBDev.Onnx.SentenceEmbeddings;
using System;
using System.Collections.Generic;

namespace OoBDev.Onnx.ImageEmbeddings;

/// <summary>How the model output becomes one vector per image.</summary>
public enum ImagePooling
{
    /// <summary>The output is already one vector per image (<c>[batch, size]</c>), such as CLIP <c>image_embeds</c> or classifier logits.</summary>
    Output,

    /// <summary>Take the first token of <c>[batch, tokens, size]</c> hidden states (class token, as DINOv2 and ViT use).</summary>
    FirstToken,

    /// <summary>Average the tokens after the first (patch tokens) of <c>[batch, tokens, size]</c> hidden states.</summary>
    MeanPatchTokens,
}

/// <summary>
/// Settings for <see cref="OnnxImageEmbeddingGenerator"/> and <see cref="OnnxImageClassifier"/>. Presets fill in
/// the model specific values; the defaults describe a 224 pixel model with Pillow style bicubic resize and no crop.
/// </summary>
public sealed class OnnxImageEmbeddingOptions
{
    /// <summary>Folder that holds the model files. A relative path is resolved against the application folder.</summary>
    public string ModelPath { get; set; } = "models";

    /// <summary>Files downloaded into <see cref="ModelPath"/> when missing.</summary>
    public IList<ModelFileSource> ModelFiles { get; set; } = [];

    /// <summary>The ONNX model file inside <see cref="ModelPath"/> (a sub folder is allowed).</summary>
    public string ModelFileName { get; set; } = "model.onnx";

    /// <summary>Name of the pixel input.</summary>
    public string InputName { get; set; } = "pixel_values";

    /// <summary>Name of the output to read; null uses the first output.</summary>
    public string? OutputName { get; set; }

    /// <summary>Side of the square image the model takes.</summary>
    public int ImageSize { get; set; } = 224;

    /// <summary>When set, resize so the shorter edge has this length and keep the aspect ratio; when null resize straight to <see cref="ImageSize"/> by <see cref="ImageSize"/>.</summary>
    public int? ResizeShortestEdge { get; set; }

    /// <summary>Crop the center <see cref="ImageSize"/> square after resizing.</summary>
    public bool CenterCrop { get; set; }

    /// <summary>Resampling filter.</summary>
    public ImageResample Resample { get; set; } = ImageResample.Bicubic;

    /// <summary>Per channel mean (R, G, B) subtracted after scaling to 0..1.</summary>
    public float[] Mean { get; set; } = [0.5f, 0.5f, 0.5f];

    /// <summary>Per channel standard deviation (R, G, B).</summary>
    public float[] Std { get; set; } = [0.5f, 0.5f, 0.5f];

    /// <summary>How the output becomes one vector.</summary>
    public ImagePooling Pooling { get; set; } = ImagePooling.Output;

    /// <summary>Scale each vector to unit length (embeddings only).</summary>
    public bool Normalize { get; set; } = true;

    /// <summary>Largest number of images in one model run.</summary>
    public int MaxBatchSize { get; set; } = 8;

    /// <summary>Largest number of simultaneous model runs.</summary>
    public int MaxConcurrentInferences { get; set; } = 1;

    /// <summary>ONNX Runtime intra-op threads; 0 lets the runtime decide.</summary>
    public int IntraOpThreads { get; set; }

    /// <summary>Largest accepted encoded image in bytes.</summary>
    public int MaxImageBytes { get; set; } = 64 * 1024 * 1024;

    /// <summary>Largest accepted <c>width * height</c> of a decoded image.</summary>
    public long MaxPixels { get; set; } = 100_000_000;

    /// <summary>Checks the settings.</summary>
    /// <exception cref="ArgumentException">A setting is out of range.</exception>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(ModelPath)) throw new ArgumentException("ModelPath is required.", nameof(ModelPath));
        if (string.IsNullOrWhiteSpace(ModelFileName)) throw new ArgumentException("ModelFileName is required.", nameof(ModelFileName));
        if (string.IsNullOrWhiteSpace(InputName)) throw new ArgumentException("InputName is required.", nameof(InputName));
        if (ImageSize < 1) throw new ArgumentException("ImageSize must be at least 1.", nameof(ImageSize));
        if (ResizeShortestEdge is < 1) throw new ArgumentException("ResizeShortestEdge must be at least 1.", nameof(ResizeShortestEdge));
        if (CenterCrop && (ResizeShortestEdge is not { } edge || edge < ImageSize))
            throw new ArgumentException("CenterCrop needs ResizeShortestEdge of at least ImageSize.", nameof(CenterCrop));
        if (Mean is not { Length: 3 }) throw new ArgumentException("Mean needs three values.", nameof(Mean));
        if (Std is not { Length: 3 } || Array.Exists(Std, s => s == 0)) throw new ArgumentException("Std needs three non-zero values.", nameof(Std));
        if (MaxBatchSize < 1) throw new ArgumentException("MaxBatchSize must be at least 1.", nameof(MaxBatchSize));
        if (MaxConcurrentInferences < 1) throw new ArgumentException("MaxConcurrentInferences must be at least 1.", nameof(MaxConcurrentInferences));
        if (IntraOpThreads < 0) throw new ArgumentException("IntraOpThreads cannot be negative.", nameof(IntraOpThreads));
        if (MaxImageBytes < 1) throw new ArgumentException("MaxImageBytes must be at least 1.", nameof(MaxImageBytes));
        if (MaxPixels < 1) throw new ArgumentException("MaxPixels must be at least 1.", nameof(MaxPixels));
    }
}
