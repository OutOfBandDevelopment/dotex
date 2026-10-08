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
    public const string Repository = "onnx-models/all-MiniLM-L6-v2-onnx";

    /// <summary>Pinned revision (commit) of <see cref="Repository"/>.</summary>
    public const string Revision = "75251058ddd779e3a744f87fdf63fb39681aec16";

    private const string ModelSha256 = "994a58868f7abacacbf2192aa0aae8f56da8c4505dbde2740c861b24426ede6b";
    private const string VocabSha256 = "07eced375cec144d27c900241f3e339478dec958f92fddbc551f295c992038a3";

    /// <summary>
    /// Default model folder: the snapshot folder of the Hugging Face hub cache (<c>models--org--name/snapshots/revision</c>),
    /// so the files are shared with Python tools and other apps. The cache root follows the Python library:
    /// <c>HF_HUB_CACHE</c>, else <c>HF_HOME/hub</c>, else <c>XDG_CACHE_HOME/huggingface/hub</c>, else <c>~/.cache/huggingface/hub</c>.
    /// Set <c>ModelPath</c> to a mapped volume in containers to keep the files between restarts.
    /// </summary>
    public static string DefaultFolder { get; } = Path.Combine(
        HubCacheRoot(), "models--" + Repository.Replace("/", "--", StringComparison.Ordinal), "snapshots", Revision);

    /// <summary>
    /// Sets the folder and the download sources to the defaults of this model.
    /// </summary>
    /// <param name="options">The options to fill.</param>
    public static void ApplyDefaults(OnnxSentenceEmbeddingOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        options.ModelPath = DefaultFolder;
        options.ModelFileName = "model.onnx";
        options.VocabFileName = "vocab.txt";
        options.ModelFiles =
        [
            new ModelFileSource { FileName = "model.onnx", Url = Url("model.onnx"), Sha256 = ModelSha256 },
            new ModelFileSource { FileName = "vocab.txt", Url = Url("vocab.txt"), Sha256 = VocabSha256 },
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

    private static string HubCacheRoot()
    {
        if (Environment.GetEnvironmentVariable("HF_HUB_CACHE") is { Length: > 0 } hub) return hub;
        if (Environment.GetEnvironmentVariable("HF_HOME") is { Length: > 0 } home) return Path.Combine(home, "hub");
        if (Environment.GetEnvironmentVariable("XDG_CACHE_HOME") is { Length: > 0 } xdg) return Path.Combine(xdg, "huggingface", "hub");
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".cache", "huggingface", "hub");
    }

    private static string Url(string file) => $"https://huggingface.co/{Repository}/resolve/{Revision}/{file}";
}
