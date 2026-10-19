using OllamaSharp.Models;
using OoBDev.AI.Models;

namespace OoBDev.Ollama;

/// <summary>
/// Default <see cref="IOllamaModelMapper"/>.
/// </summary>
public class OllamaModelMapper : IOllamaModelMapper
{
    /// <inheritdoc />
#pragma warning disable CS0618 // Ollama deprecates generate context, but it is what carries the CompletionRequest.Context contract
    public GenerateRequest Map(CompletionRequest request, string defaultModel) =>
        new()
        {
            Model = request.Model ?? defaultModel,
            Prompt = request.Prompt,
            System = request.System,
            Template = request.Template,
            Context = request.Context,
            Images = request.Images,
            Stream = true,
        };
#pragma warning restore CS0618
}
