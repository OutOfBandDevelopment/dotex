using OoBDev.Search.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;

namespace OoBDev.Search.Semantic;

/// <summary>
/// Tags each stored vector with the model that produced it, so a model change can be detected and the
/// affected rows re-embedded. The tag lives in the vector's metadata, so no store needs a schema change.
/// </summary>
public static class EmbeddingVersion
{
    /// <summary>The metadata key that holds the tag.</summary>
    public const string MetadataKey = "embeddingModel";

    /// <summary>Builds the tag for a model: its id and vector length, for example <c>all-MiniLM-L6-v2/384</c>.</summary>
    /// <param name="modelId">The model identifier (for example the model file name or preset key).</param>
    /// <param name="dimensions">The number of dimensions of the vectors the model produces.</param>
    /// <returns>The tag.</returns>
    public static string Tag(string modelId, int dimensions)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(modelId);
        return string.Create(CultureInfo.InvariantCulture, $"{modelId}/{dimensions}");
    }

    /// <summary>Returns a copy of <paramref name="metadata"/> carrying the tag; an existing tag is replaced.</summary>
    /// <param name="metadata">The metadata to stamp, or null for none.</param>
    /// <param name="tag">The tag from <see cref="Tag"/>.</param>
    /// <returns>A new dictionary with the tag set.</returns>
    public static IDictionary<string, object> Stamp(IDictionary<string, object>? metadata, string tag)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tag);
        var stamped = metadata == null
            ? new Dictionary<string, object>(StringComparer.Ordinal)
            : new Dictionary<string, object>(metadata, StringComparer.Ordinal);
        stamped[MetadataKey] = tag;
        return stamped;
    }

    /// <summary>Reads the tag from a result's metadata; null when the vector was stored before tagging.</summary>
    /// <param name="result">A search or list result.</param>
    /// <returns>The tag or null.</returns>
    public static string? Read(SearchResultModel result)
    {
        ArgumentNullException.ThrowIfNull(result);
        return result.MetaData != null && result.MetaData.TryGetValue(MetadataKey, out var value)
            ? Convert.ToString(value, CultureInfo.InvariantCulture)
            : null;
    }

    /// <summary>True when the vector was not produced by the current model; an untagged vector counts as stale.</summary>
    /// <param name="result">A search or list result.</param>
    /// <param name="currentTag">The tag of the model in use.</param>
    /// <returns>Whether the vector needs re-embedding.</returns>
    public static bool IsStale(SearchResultModel result, string currentTag) =>
        !string.Equals(Read(result), currentTag, StringComparison.Ordinal);

    /// <summary>The ids of the results that need re-embedding for the current model.</summary>
    /// <param name="results">Results from <c>ListAsync</c>.</param>
    /// <param name="currentTag">The tag of the model in use.</param>
    /// <returns>The stale item ids.</returns>
    public static IReadOnlyList<string> StaleIds(IEnumerable<SearchResultModel> results, string currentTag)
    {
        ArgumentNullException.ThrowIfNull(results);
        return [.. results.Where(r => IsStale(r, currentTag)).Select(r => r.ItemId)];
    }

    /// <summary>
    /// Re-embeds the stale rows of a store: for each stale result the callback supplies a new vector (it knows the source
    /// text; null skips the row), the new row is stored with the current tag, and only then is the old row deleted, so a
    /// failure never loses a row (it can leave a duplicate that the next run replaces).
    /// </summary>
    /// <param name="store">The vector store.</param>
    /// <param name="currentTag">The tag of the model in use.</param>
    /// <param name="embed">Produces the new vector for a stale result, or null to skip it.</param>
    /// <returns>The number of rows replaced.</returns>
    public static async Task<int> ReembedAsync(IVectorStore store, string currentTag, Func<SearchResultModel, Task<ReadOnlyMemory<float>?>> embed)
    {
        ArgumentNullException.ThrowIfNull(store);
        ArgumentNullException.ThrowIfNull(embed);
        var stale = new List<SearchResultModel>();
        await foreach (var row in store.ListAsync().ConfigureAwait(false))
        {
            if (IsStale(row, currentTag))
            {
                stale.Add(row);
            }
        }

        var replaced = 0;
        foreach (var row in stale)
        {
            var vector = await embed(row).ConfigureAwait(false);
            if (vector is null)
            {
                continue;
            }

            await store.StoreVectorsAsync([vector.Value], Stamp(row.MetaData, currentTag)).ConfigureAwait(false);
            await store.DeleteAsync([row.ItemId]).ConfigureAwait(false);
            replaced++;
        }

        return replaced;
    }
}
