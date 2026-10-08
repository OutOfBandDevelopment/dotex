using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace OoBDev.Onnx.SentenceEmbeddings.Tests;

/// <summary>
/// Finds the all-MiniLM-L6-v2 model files (git submodule of the SBert adapter) from the test output folder.
/// </summary>
internal static class TestModel
{
    private static readonly string[] _relative = ["src", "ExternalServices", "SBert", "OoBDev.SBert.AllMiniLML6v2Sharp", "model"];

    public static string? Folder
    {
        get
        {
            for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir is not null; dir = dir.Parent)
            {
                var candidate = Path.Combine([dir.FullName, .. _relative]);
                if (File.Exists(Path.Combine(candidate, "model.onnx")) && File.Exists(Path.Combine(candidate, "vocab.txt"))) return candidate;
            }
            return null;
        }
    }

    public static string RequireFolder() =>
        Folder ?? throw new AssertInconclusiveException("The all-MiniLM-L6-v2 model files are not checked out (git submodule).");

    public static OnnxSentenceEmbeddingGenerator CreateGenerator(Action<OnnxSentenceEmbeddingOptions>? configure = null)
    {
        var options = new OnnxSentenceEmbeddingOptions { ModelPath = RequireFolder() };
        configure?.Invoke(options);
        return new OnnxSentenceEmbeddingGenerator(Options.Create(options), NullLogger<OnnxSentenceEmbeddingGenerator>.Instance);
    }
}
