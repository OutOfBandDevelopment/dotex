using Microsoft.Extensions.Logging;
using OoBDev.Onnx.SentenceEmbeddings;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.SBert.NomicEmbedTextV1_5;

/// <summary>
/// The nomic-embed-text-v1.5 model files: a pinned Hugging Face revision with SHA-256 hashes, downloaded on first use.
/// </summary>
public static class NomicEmbedTextV1_5Model
{
    /// <summary>Hugging Face repository the files come from (the same one the Python libraries use).</summary>
    public const string Repository = "nomic-ai/nomic-embed-text-v1.5";

    /// <summary>Pinned revision (commit) of <see cref="Repository"/>.</summary>
    public const string Revision = "e9b6763023c676ca8431644204f50c2b100d9aab";

    private const string ModelSha256 = "147d5aa88c2101237358e17796cf3a227cead1ec304ec34b465bb08e9d952965";
    private const string VocabSha256 = "07eced375cec144d27c900241f3e339478dec958f92fddbc551f295c992038a3";

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
        options.MaxSequenceLength = 512;
        options.Prefix = "search_document: ";
        options.LayerNormalize = true;
        options.SupportsDimensionTruncation = true;
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
