using System;
using System.IO;

namespace OoBDev.Onnx.SentenceEmbeddings;

/// <summary>
/// Locations and download sources in the Hugging Face hub cache layout, so the files are shared with the Python
/// libraries (<c>huggingface_hub</c>, <c>sentence-transformers</c>) and with other apps on the same machine.
/// </summary>
public static class HuggingFaceHubCache
{
    /// <summary>
    /// The cache root, following the Python library: <c>HF_HUB_CACHE</c>, else <c>HF_HOME/hub</c>,
    /// else <c>XDG_CACHE_HOME/huggingface/hub</c>, else <c>~/.cache/huggingface/hub</c>.
    /// </summary>
    public static string Root()
    {
        if (Environment.GetEnvironmentVariable("HF_HUB_CACHE") is { Length: > 0 } hub) return hub;
        if (Environment.GetEnvironmentVariable("HF_HOME") is { Length: > 0 } home) return Path.Combine(home, "hub");
        if (Environment.GetEnvironmentVariable("XDG_CACHE_HOME") is { Length: > 0 } xdg) return Path.Combine(xdg, "huggingface", "hub");
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".cache", "huggingface", "hub");
    }

    /// <summary>
    /// The snapshot folder of a repository revision: <c>models--org--name/snapshots/revision</c>.
    /// </summary>
    /// <param name="repository">Repository id such as <c>sentence-transformers/all-mpnet-base-v2</c>.</param>
    /// <param name="revision">Commit hash of the revision.</param>
    public static string SnapshotFolder(string repository, string revision)
    {
        ArgumentException.ThrowIfNullOrEmpty(repository);
        ArgumentException.ThrowIfNullOrEmpty(revision);
        return Path.Combine(Root(), "models--" + repository.Replace("/", "--", StringComparison.Ordinal), "snapshots", revision);
    }

    /// <summary>
    /// A file of a pinned revision, downloaded into the snapshot folder when missing.
    /// </summary>
    /// <param name="repository">Repository id.</param>
    /// <param name="revision">Commit hash of the revision.</param>
    /// <param name="fileName">Path of the file inside the repository, for example <c>onnx/model.onnx</c>.</param>
    /// <param name="sha256">Expected SHA-256 of the file (hex).</param>
    public static ModelFileSource File(string repository, string revision, string fileName, string sha256) => new()
    {
        FileName = fileName,
        Url = $"https://huggingface.co/{repository}/resolve/{revision}/{fileName}",
        Sha256 = sha256,
    };
}
