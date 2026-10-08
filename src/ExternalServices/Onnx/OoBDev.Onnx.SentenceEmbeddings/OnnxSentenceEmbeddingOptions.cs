using System;
using System.Collections.Generic;

namespace OoBDev.Onnx.SentenceEmbeddings;

/// <summary>
/// Settings for <see cref="OnnxSentenceEmbeddingGenerator"/>. Model presets derive from this class and set their defaults.
/// </summary>
public class OnnxSentenceEmbeddingOptions
{
    /// <summary>Folder with <c>model.onnx</c> and <c>vocab.txt</c>; relative paths resolve against <see cref="AppContext.BaseDirectory"/>.</summary>
    public string ModelPath { get; set; } = "model";

    /// <summary>
    /// Files downloaded into <see cref="ModelPath"/> on first use when they are missing (verified by SHA-256).
    /// Map <see cref="ModelPath"/> to a volume in a container to keep them between restarts.
    /// </summary>
    public IList<ModelFileSource> ModelFiles { get; set; } = [];

    /// <summary>File name of the ONNX model inside <see cref="ModelPath"/>.</summary>
    public string ModelFileName { get; set; } = "model.onnx";

    /// <summary>File name of the WordPiece vocabulary inside <see cref="ModelPath"/>.</summary>
    public string VocabFileName { get; set; } = "vocab.txt";

    /// <summary>Longest token sequence (including special tokens); longer input is truncated.</summary>
    public int MaxSequenceLength { get; set; } = 256;

    /// <summary>Most values sent through the model in one run.</summary>
    public int MaxBatchSize { get; set; } = 32;

    /// <summary>Most model runs in flight at once; a burst of callers queues instead of oversubscribing the machine.</summary>
    public int MaxConcurrentInferences { get; set; } = Environment.ProcessorCount;

    /// <summary>Threads ONNX Runtime uses inside one run; zero lets the runtime choose.</summary>
    public int IntraOpThreads { get; set; }

    /// <summary>Lower-case text before tokenizing (true for uncased vocabularies).</summary>
    public bool LowerCase { get; set; } = true;

    /// <summary>Name of the token id input.</summary>
    public string InputIdsName { get; set; } = "input_ids";

    /// <summary>Name of the attention mask input.</summary>
    public string AttentionMaskName { get; set; } = "attention_mask";

    /// <summary>Name of the token type id input, or null when the model has none.</summary>
    public string? TokenTypeIdsName { get; set; } = "token_type_ids";

    /// <summary>Pooling over the token vectors.</summary>
    public EmbeddingPooling Pooling { get; set; } = EmbeddingPooling.Mean;

    /// <summary>Scale each vector to unit length.</summary>
    public bool Normalize { get; set; } = true;

    /// <summary>Optional text added in front of every value (for example <c>query: </c>).</summary>
    public string? Prefix { get; set; }

    /// <summary>Output size after truncating and re-normalising; only valid when <see cref="SupportsDimensionTruncation"/> is true.</summary>
    public int? Dimensions { get; set; }

    /// <summary>True for models trained so that a vector prefix is still a good embedding (Matryoshka).</summary>
    public bool SupportsDimensionTruncation { get; set; }

    /// <summary>Checks the settings and throws <see cref="ArgumentException"/> with the first problem found.</summary>
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(ModelPath)) throw new ArgumentException("ModelPath is required.", nameof(ModelPath));
        if (MaxSequenceLength < 3) throw new ArgumentException("MaxSequenceLength must be at least 3.", nameof(MaxSequenceLength));
        if (MaxBatchSize < 1) throw new ArgumentException("MaxBatchSize must be at least 1.", nameof(MaxBatchSize));
        if (MaxConcurrentInferences < 1) throw new ArgumentException("MaxConcurrentInferences must be at least 1.", nameof(MaxConcurrentInferences));
        if (IntraOpThreads < 0) throw new ArgumentException("IntraOpThreads cannot be negative.", nameof(IntraOpThreads));
        if (Dimensions is { } d)
        {
            if (!SupportsDimensionTruncation) throw new ArgumentException("This model is not trained for truncated vectors; Dimensions is not supported.", nameof(Dimensions));
            if (d < 1) throw new ArgumentException("Dimensions must be at least 1.", nameof(Dimensions));
        }
    }
}
