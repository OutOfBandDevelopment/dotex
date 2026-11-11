using Azure.Search.Documents;
using Azure.Search.Documents.Indexes.Models;
using Azure.Search.Documents.Models;
using OoBDev.Search.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.Json;

namespace OoBDev.Microsoft.Azure.AISearch;

/// <summary>
/// Maps between the vector store contract and Azure AI Search documents and index definitions.
/// </summary>
internal static class AzureAISearchMapper
{
    public static SearchIndex CreateIndex(string name, int dimensions) =>
        new(name)
        {
            Fields =
            {
                new SimpleField(AzureAISearchGlobals.IdField, SearchFieldDataType.String) { IsKey = true, IsFilterable = true },
                new SimpleField(AzureAISearchGlobals.PayloadField, SearchFieldDataType.String),
                new SearchField(AzureAISearchGlobals.VectorField, SearchFieldDataType.Collection(SearchFieldDataType.Single))
                {
                    IsSearchable = true,
                    VectorSearchDimensions = dimensions,
                    VectorSearchProfileName = AzureAISearchGlobals.ProfileName,
                },
            },
            VectorSearch = new VectorSearch
            {
                Algorithms = { new HnswAlgorithmConfiguration(AzureAISearchGlobals.AlgorithmName) },
                Profiles = { new VectorSearchProfile(AzureAISearchGlobals.ProfileName, AzureAISearchGlobals.AlgorithmName) },
            },
        };

    public static SearchDocument ToDocument(ReadOnlyMemory<float> embedding, IEnumerable<KeyValuePair<string, object>> metadata) =>
        new()
        {
            [AzureAISearchGlobals.IdField] = Guid.NewGuid().ToString("N"),
            [AzureAISearchGlobals.VectorField] = embedding.ToArray(),
            [AzureAISearchGlobals.PayloadField] = JsonSerializer.Serialize(
                metadata.GroupBy(m => m.Key, StringComparer.Ordinal).ToDictionary(g => g.Key, g => g.Last().Value, StringComparer.Ordinal)),
        };

    public static SearchOptions CreateNeighborOptions(ReadOnlyMemory<float> find, int top) =>
        new()
        {
            Size = top,
            Select = { AzureAISearchGlobals.IdField, AzureAISearchGlobals.PayloadField },
            VectorSearch = new VectorSearchOptions
            {
                Queries = { new VectorizedQuery(find) { KNearestNeighborsCount = top, Fields = { AzureAISearchGlobals.VectorField } } },
            },
        };

    public static SearchResultModel ToResult(SearchDocument document, double? score) =>
        new()
        {
            Score = (float)(score ?? 0),
            ItemId = document.TryGetValue(AzureAISearchGlobals.IdField, out var id) ? Convert.ToString(id, CultureInfo.InvariantCulture) ?? "" : "",
            MetaData = ReadPayload(document.TryGetValue(AzureAISearchGlobals.PayloadField, out var payload) ? payload as string : null),
        };

    public static Dictionary<string, object> ReadPayload(string? json)
    {
        var result = new Dictionary<string, object>(StringComparer.Ordinal);
        if (string.IsNullOrWhiteSpace(json))
            return result;
        using var doc = JsonDocument.Parse(json);
        foreach (var property in doc.RootElement.EnumerateObject())
            if (ToValue(property.Value) is { } value)
                result[property.Name] = value;
        return result;
    }

    private static object? ToValue(JsonElement element) =>
        element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number => element.TryGetInt64(out var l) ? (object)l : element.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Array => element.EnumerateArray().Select(ToValue).ToList(),
            JsonValueKind.Object => element.GetRawText(),
            _ => null,
        };
}
