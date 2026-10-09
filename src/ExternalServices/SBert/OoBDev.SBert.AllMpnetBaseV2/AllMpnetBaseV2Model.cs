using Microsoft.Extensions.Logging;
using OoBDev.Onnx.SentenceEmbeddings;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.SBert.AllMpnetBaseV2;

/// <summary>
/// The all-mpnet-base-v2 model files: a pinned Hugging Face revision with SHA-256 hashes, downloaded on first use.
/// </summary>
public static class AllMpnetBaseV2Model
{
    /// <summary>Hugging Face repository the files come from (the same one the Python libraries use).</summary>
    public const string Repository = "sentence-transformers/all-mpnet-base-v2";

    /// <summary>Pinned revision (commit) of <see cref="Repository"/>.</summary>
    public const string Revision = "e8c3b32edf5434bc2275fc9bab85f82640a19130";

    private const string ModelSha256 = "74187b16d9c946fea252e120cfd7a12c5779d8b8b86838a2e4c56573c47941bd";
    private const string VocabSha256 = "dbd90cb94e2247bd4d4ccaecbf616d2290e66691d7d5e5bb81f063c2d0649ada";

    /// <summary>
    /// Default model folder: the snapshot folder of the Hugging Face hub cache, shared with Python tools and other apps
    /// (see <see cref="HuggingFaceHubCache"/>). Set <c>ModelPath</c> to a mapped volume in containers.
    /// </summary>
    public static string DefaultFolder { get; } = HuggingFaceHubCache.SnapshotFolder(Repository, Revision);

    /// <summary>
    /// Sets the folder, the model settings and the download sources to the defaults of this model.
    /// </summary>
    /// <param name="options">The options to fill.</param>
    public static void ApplyDefaults(OnnxSentenceEmbeddingOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        options.ModelPath = DefaultFolder;
        options.ModelFileName = "onnx/model.onnx";
        options.VocabFileName = "vocab.txt";
        options.MaxSequenceLength = 384;
        options.ClsToken = "<s>";
        options.SepToken = "</s>";
        options.UnkToken = "[UNK]";
        options.PadToken = "<pad>";
        options.MaskToken = "<mask>";
        options.TokenTypeIdsName = null;
        options.ModelFiles =
        [
            HuggingFaceHubCache.File(Repository, Revision, "onnx/model.onnx", ModelSha256),
            HuggingFaceHubCache.File(Repository, Revision, "vocab.txt", VocabSha256),
        ];
    }

    /// <summary>
    /// Downloads the model files that are missing, for example at startup.
    /// </summary>
    /// <param name="options">Options filled by <see cref="ApplyDefaults"/> and the configuration.</param>
    /// <param name="logger">Logger.</param>
    /// <param name="cancellationToken">Cancellation.</param>
    /// <returns>The model folder.</returns>
    public static Task<string> EnsureAsync(OnnxSentenceEmbeddingOptions options, ILogger logger, CancellationToken cancellationToken = default) =>
        OnnxSentenceEmbeddingGenerator.EnsureModelAsync(options, logger, cancellationToken);
}
