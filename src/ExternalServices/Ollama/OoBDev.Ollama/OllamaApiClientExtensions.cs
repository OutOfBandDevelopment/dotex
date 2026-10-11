using Microsoft.Extensions.Logging;
using OllamaSharp;
using OllamaSharp.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OoBDev.Ollama;

/// <summary>
/// Provides extension methods for the <see cref="OllamaApiClient"/> class.
/// </summary>
public static partial class OllamaApiClientExtensions
{
    /// <summary>
    /// Ensures that the specified model exists in the Ollama API client.
    /// If the model does not exist locally, it is copied from the 'llama2' model.
    /// </summary>
    /// <param name="ollama">The Ollama API client.</param>
    /// <param name="modelName">The name of the model to ensure exists.</param>
    /// <param name="logger">The logger instance for logging information.</param>
    /// <returns>The Ollama API client.</returns>
    public static async Task<IOllamaApiClient> EnsureModelExistsAsync(
        this IOllamaApiClient ollama,
        string modelName,
        ILogger logger
        )
    {
        LogListModels(logger);
        var models = await ollama.ListLocalModelsAsync();
        foreach (var model in models)
            LogModel(logger, model.Name, model.Size);

        LogCreateMaybe(logger);
        if (models.Any(m => !m.Name.StartsWith(modelName, StringComparison.Ordinal)))
            await ollama.CopyModelAsync("llama2", modelName); //TODO: fix this
        LogSelectModel(logger, modelName);

        //ollama.SelectedModel = modelName;

        return ollama;
    }

    /// <summary>
    /// Synchronously retrieves the embedding for the specified text using the specified model.
    /// </summary>
    /// <param name="client">The Ollama API client.</param>
    /// <param name="text">The text for which the embedding is requested.</param>
    /// <param name="modelName">The name of the model to use for embedding.</param>
    /// <returns>The embedding as an array of floats.</returns>
    public static ReadOnlyMemory<float> GetEmbeddingSingle(
        this IOllamaApiClient client,
        string text,
        string modelName) =>
                client.GetEmbeddingSingleAsync(text, modelName).Result;

    /// <summary>
    /// Asynchronously retrieves the embedding for the specified text using the specified model.
    /// </summary>
    /// <param name="client">The Ollama API client.</param>
    /// <param name="text">The text for which the embedding is requested.</param>
    /// <param name="modelName">The name of the model to use for embedding.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains the embedding as an array of floats.</returns>
    public static async Task<ReadOnlyMemory<float>> GetEmbeddingSingleAsync(
        this IOllamaApiClient client,
        string text,
        string modelName
        )
    {
        var doubles = await client.GetEmbeddingDoubleAsync(text, modelName);
        var floats = Array.ConvertAll(doubles.ToArray(), Convert.ToSingle);
        return floats;
    }

    /// <summary>
    /// Asynchronously retrieves the embedding for the specified text using the specified model.
    /// </summary>
    /// <param name="client">The Ollama API client.</param>
    /// <param name="text">The text for which the embedding is requested.</param>
    /// <param name="modelName">The name of the model to use for embedding.</param>
    /// <returns>A task representing the asynchronous operation. The task result contains the embedding as an array of doubles.</returns>
    public static async Task<ReadOnlyMemory<double>> GetEmbeddingDoubleAsync(
        this IOllamaApiClient client,
        string text,
        string modelName)
    {
        var request = new EmbedRequest
        {
            Model = modelName,
            Input = [text],
        };
        var response = await client.EmbedAsync(request);
        return response.Embeddings[0].Select(Convert.ToDouble).ToArray();
    }

    /// <summary>
    /// Synchronously retrieves the embedding for the specified text using the specified model.
    /// </summary>
    /// <param name="client">The Ollama API client.</param>
    /// <param name="text">The text for which the embedding is requested.</param>
    /// <param name="modelName">The name of the model to use for embedding.</param>
    /// <returns>The embedding as an array of doubles.</returns>
    public static ReadOnlyMemory<double> GetEmbeddingDouble(
        this IOllamaApiClient client,
        string text,
        string modelName) =>
            client.GetEmbeddingDoubleAsync(text, modelName).Result;

    [LoggerMessage(EventId = 1, Level = LogLevel.Information, Message = "list models")]
    private static partial void LogListModels(ILogger logger);

    [LoggerMessage(EventId = 2, Level = LogLevel.Information, Message = "model: {model}, size: {size} ")]
    private static partial void LogModel(ILogger logger, string model, long size);

    [LoggerMessage(EventId = 3, Level = LogLevel.Information, Message = "create maybe")]
    private static partial void LogCreateMaybe(ILogger logger);

    [LoggerMessage(EventId = 4, Level = LogLevel.Information, Message = "Select model: {modelName}")]
    private static partial void LogSelectModel(ILogger logger, string modelName);
}
