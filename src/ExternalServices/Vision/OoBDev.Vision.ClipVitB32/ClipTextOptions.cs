using System;

namespace OoBDev.Vision.ClipVitB32;

/// <summary>Settings of the CLIP text tower.</summary>
public sealed class ClipTextOptions
{
    /// <summary>Folder that holds the model files. A relative path is resolved against the application folder.</summary>
    public string ModelPath { get; set; } = "models";

    /// <summary>The text tower ONNX file inside <see cref="ModelPath"/>.</summary>
    public string ModelFileName { get; set; } = "onnx/text_model.onnx";

    /// <summary>Tokenizer vocabulary file.</summary>
    public string VocabFileName { get; set; } = "vocab.json";

    /// <summary>Tokenizer merges file.</summary>
    public string MergesFileName { get; set; } = "merges.txt";

    /// <summary>Name of the token id input.</summary>
    public string InputName { get; set; } = "input_ids";

    /// <summary>Name of the embedding output.</summary>
    public string OutputName { get; set; } = "text_embeds";

    /// <summary>Largest number of token ids including start and end (CLIP was trained with 77).</summary>
    public int MaxSequenceLength { get; set; } = 77;

    /// <summary>Largest number of texts in one model run.</summary>
    public int MaxBatchSize { get; set; } = 16;

    /// <summary>Largest number of simultaneous model runs.</summary>
    public int MaxConcurrentInferences { get; set; } = 1;

    /// <summary>Scale each vector to unit length.</summary>
    public bool Normalize { get; set; } = true;

    /// <summary>Checks the settings.</summary>
    /// <exception cref="ArgumentException">A setting is out of range.</exception>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(ModelPath)) throw new ArgumentException("ModelPath is required.", nameof(ModelPath));
        if (string.IsNullOrWhiteSpace(ModelFileName)) throw new ArgumentException("ModelFileName is required.", nameof(ModelFileName));
        if (string.IsNullOrWhiteSpace(VocabFileName)) throw new ArgumentException("VocabFileName is required.", nameof(VocabFileName));
        if (string.IsNullOrWhiteSpace(MergesFileName)) throw new ArgumentException("MergesFileName is required.", nameof(MergesFileName));
        if (string.IsNullOrWhiteSpace(InputName)) throw new ArgumentException("InputName is required.", nameof(InputName));
        if (string.IsNullOrWhiteSpace(OutputName)) throw new ArgumentException("OutputName is required.", nameof(OutputName));
        if (MaxSequenceLength is < 3 or > 77) throw new ArgumentException("MaxSequenceLength must be 3 to 77.", nameof(MaxSequenceLength));
        if (MaxBatchSize < 1) throw new ArgumentException("MaxBatchSize must be at least 1.", nameof(MaxBatchSize));
        if (MaxConcurrentInferences < 1) throw new ArgumentException("MaxConcurrentInferences must be at least 1.", nameof(MaxConcurrentInferences));
    }
}
