using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using OoBDev.SBert.AllMiniLmL6V2;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace OoBDev.Onnx.SentenceEmbeddings.Tests;

/// <summary>
/// The all-MiniLM-L6-v2 model files, downloaded on first use into the shared Hugging Face cache.
/// </summary>
internal static class TestModel
{
    public static OnnxSentenceEmbeddingOptions CreateOptions()
    {
        var options = new OnnxSentenceEmbeddingOptions();
        AllMiniLmL6V2Model.ApplyDefaults(options);
        return options;
    }

    public static string RequireFolder()
    {
        try
        {
            return AllMiniLmL6V2Model.EnsureAsync(CreateOptions(), NullLogger.Instance).GetAwaiter().GetResult();
        }
        catch (global::System.Net.Http.HttpRequestException ex)
        {
            throw new AssertInconclusiveException("The all-MiniLM-L6-v2 model could not be downloaded: " + ex.Message);
        }
    }

    public static OnnxSentenceEmbeddingGenerator CreateGenerator(Action<OnnxSentenceEmbeddingOptions>? configure = null)
    {
        RequireFolder();
        var options = CreateOptions();
        configure?.Invoke(options);
        return new OnnxSentenceEmbeddingGenerator(Microsoft.Extensions.Options.Options.Create(options), NullLogger<OnnxSentenceEmbeddingGenerator>.Instance);
    }
}
