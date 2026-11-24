using System.Collections.Generic;
using System.Text.Json;

namespace OoBDev.Data.DesiredState;

/// <summary>
/// Optional layer 3: the extra objects a schema diff cannot model, named instead of scripted. JSON form:
/// <c>{ "extensions": ["pgcrypto"], "queues": ["orders"], "streams": ["order_events"] }</c>.
/// </summary>
public sealed record FeatureManifest(IReadOnlyList<string> Extensions, IReadOnlyList<string> Queues, IReadOnlyList<string> Streams)
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

    public static FeatureManifest Parse(string json)
    {
        var raw = JsonSerializer.Deserialize<RawManifest>(json, JsonOptions) ?? new RawManifest();
        return new FeatureManifest(raw.Extensions ?? [], raw.Queues ?? [], raw.Streams ?? []);
    }

    private sealed class RawManifest
    {
        public List<string>? Extensions { get; set; }

        public List<string>? Queues { get; set; }

        public List<string>? Streams { get; set; }
    }
}
