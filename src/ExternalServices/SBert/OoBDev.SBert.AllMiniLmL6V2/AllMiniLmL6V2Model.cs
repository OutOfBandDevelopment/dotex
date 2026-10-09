using Microsoft.Extensions.Logging;
using OoBDev.Onnx.SentenceEmbeddings;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace OoBDev.SBert.AllMiniLmL6V2;

/// <summary>
/// The all-MiniLM-L6-v2 model files: a pinned Hugging Face revision with SHA-256 hashes, downloaded on first use.
/// </summary>
public static class AllMiniLmL6V2Model
{
    /// <summary>Hugging Face repository the files come from.</summary>
    public const string Repository = "sentence-transformers/all-MiniLM-L6-v2";

    /// <summary>Pinned revision (commit) of <see cref="Repository"/>.</summary>
    public const string Revision = "1110a243fdf4706b3f48f1d95db1a4f5529b4d41";

    private const string ModelSha256 = "6fd5d72fe4589f189f8ebc006442dbb529bb7ce38f8082112682524616046452";
    private const string VocabSha256 = "07eced375cec144d27c900241f3e339478dec958f92fddbc551f295c992038a3";

    /// <summary>
    /// Default model folder: the snapshot folder of the Hugging Face hub cache (<c>models--org--name/snapshots/revision</c>),
    /// so the files are shared with Python tools and other apps. The cache root follows the Python library:
    /// <c>HF_HUB_CACHE</c>, else <c>HF_HOME/hub</c>, else <c>XDG_CACHE_HOME/huggingface/hub</c>, else <c>~/.cache/huggingface/hub</c>.
    /// Set <c>ModelPath</c> to a mapped volume in containers to keep the files between restarts.
    /// </summary>
    public static string DefaultFolder { get; } = HuggingFaceHubCache.SnapshotFolder(Repository, Revision);

    /// <summary>
    /// Sets the folder and the download sources to the defaults of this model.
    /// </summary>
    /// <param name="options">The options to fill.</param>
    public static void ApplyDefaults(OnnxSentenceEmbeddingOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        options.ModelPath = DefaultFolder;
        options.ModelFileName = "onnx/model.onnx";
        options.VocabFileName = "vocab.txt";
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
