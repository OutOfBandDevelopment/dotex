using System;
using System.Collections.Generic;
using System.Text.Json;

namespace OoBDev.Communications.EntityFrameworkCore;

/// <summary>Stores request data as JSON and gives back plain values (strings, numbers, booleans) so composers can read them.</summary>
internal static class RequestDataJson
{
    public static string Write(IReadOnlyDictionary<string, object?> data) => JsonSerializer.Serialize(data);

    public static IReadOnlyDictionary<string, object?> Read(string json)
    {
        var result = new Dictionary<string, object?>(StringComparer.Ordinal);
        using var document = JsonDocument.Parse(json);
        foreach (var property in document.RootElement.EnumerateObject())
        {
            result[property.Name] = Plain(property.Value);
        }

        return result;
    }

    private static object? Plain(JsonElement value) => value.ValueKind switch
    {
        JsonValueKind.String => value.GetString(),
        JsonValueKind.Number => value.TryGetInt64(out var whole) ? (object)whole : value.GetDouble(),
        JsonValueKind.True => true,
        JsonValueKind.False => false,
        JsonValueKind.Null or JsonValueKind.Undefined => null,
        _ => value.Clone(),
    };
}
