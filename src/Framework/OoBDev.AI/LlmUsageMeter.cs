using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Linq;

namespace OoBDev.AI;

/// <summary>
/// Adds up estimated LLM usage per model, in memory, and publishes it as the <c>oobdev.ai.estimated_tokens</c> counter
/// (tag <c>model</c>, tag <c>direction</c> = <c>prompt</c> or <c>completion</c>) on the <c>OoBDev.AI</c> meter so OpenTelemetry can export it.
/// </summary>
public sealed class LlmUsageMeter
{
    /// <summary>The meter name to add to the OpenTelemetry meter list.</summary>
    public const string MeterName = "OoBDev.AI";

    private static readonly Meter Meter = new(MeterName);
    private static readonly Counter<long> Tokens = Meter.CreateCounter<long>("oobdev.ai.estimated_tokens", "{token}", "Estimated tokens (characters divided by four), not provider-billed counts");

    private readonly ConcurrentDictionary<string, (long Calls, long Prompt, long Completion)> usage = new();

    /// <summary>Records one finished call.</summary>
    public void Record(string? model, string? prompt, string? completion)
    {
        var name = string.IsNullOrWhiteSpace(model) ? "(default)" : model;
        var prompted = TokenEstimate.Of(prompt);
        var completed = TokenEstimate.Of(completion);
        usage.AddOrUpdate(name, (1, prompted, completed), (_, u) => (u.Calls + 1, u.Prompt + prompted, u.Completion + completed));
        Tokens.Add(prompted, new KeyValuePair<string, object?>("model", name), new KeyValuePair<string, object?>("direction", "prompt"));
        Tokens.Add(completed, new KeyValuePair<string, object?>("model", name), new KeyValuePair<string, object?>("direction", "completion"));
    }

    /// <summary>The usage so far, by model name.</summary>
    public IReadOnlyList<LlmUsage> Snapshot() =>
        usage.OrderBy(u => u.Key, StringComparer.Ordinal)
            .Select(u => new LlmUsage(u.Key, u.Value.Calls, u.Value.Prompt, u.Value.Completion))
            .ToList();
}
