namespace OoBDev.AI;

/// <summary>
/// Usage of one model. Token counts are estimates (<see cref="TokenEstimate"/>), so label them approximate wherever they are shown.
/// </summary>
/// <param name="Model">The model name, or <c>(default)</c> when the request named none.</param>
/// <param name="Calls">Completed calls.</param>
/// <param name="EstimatedPromptTokens">Estimated tokens sent.</param>
/// <param name="EstimatedCompletionTokens">Estimated tokens received.</param>
public sealed record LlmUsage(string Model, long Calls, long EstimatedPromptTokens, long EstimatedCompletionTokens)
{
    /// <summary>Estimated tokens in both directions.</summary>
    public long EstimatedTotalTokens => EstimatedPromptTokens + EstimatedCompletionTokens;
}
