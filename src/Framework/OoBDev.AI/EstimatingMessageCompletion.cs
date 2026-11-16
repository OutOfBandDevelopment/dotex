using OoBDev.AI.Models;
using System.Threading.Tasks;

namespace OoBDev.AI;

/// <summary>
/// Decorates an <see cref="IMessageCompletion"/> and records estimated token usage per model in an <see cref="LlmUsageMeter"/>.
/// The interface is unchanged: the numbers are characters divided by four, not what the provider billed.
/// </summary>
/// <param name="inner">The completion service that does the work.</param>
/// <param name="meter">Where the usage is added up.</param>
public sealed class EstimatingMessageCompletion(IMessageCompletion inner, LlmUsageMeter meter) : IMessageCompletion
{
    /// <inheritdoc />
    public async Task<string> GetCompletionAsync(string modelName, string prompt)
    {
        var response = await inner.GetCompletionAsync(modelName, prompt);
        meter.Record(modelName, prompt, response);
        return response;
    }

    /// <inheritdoc />
    public async Task<CompletionResponse> GetCompletionAsync(CompletionRequest model)
    {
        var response = await inner.GetCompletionAsync(model);
        meter.Record(model.Model, string.Concat(model.System, model.Prompt), response.Response);
        return response;
    }
}
