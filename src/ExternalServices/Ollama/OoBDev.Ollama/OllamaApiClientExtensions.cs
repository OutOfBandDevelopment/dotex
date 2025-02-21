using Microsoft.Extensions.Logging;
using OllamaSharp;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace OoBDev.Ollama;

/// <summary>
/// Provides extension methods for the <see cref="OllamaApiClient"/> class.
/// </summary>
public static class OllamaApiClientExtensions
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
        logger.LogInformation("list models");
        var models = await ollama.ListLocalModelsAsync();
        foreach (var model in models)
            logger.LogInformation("model: {model}, size: {size} ", model.Name, model.Size);

        logger.LogInformation("create maybe");
        if (models.Any(m => !m.Name.StartsWith(modelName)))
            await ollama.CopyModelAsync("llama2", modelName); //TODO: fix this
        logger.LogInformation("Select model: {modelName}", modelName);

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
        string modelName) =>
            await client.GetEmbeddingDoubleAsync(text, modelName);

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
            client.GetEmbeddingDouble(text, modelName);
}
