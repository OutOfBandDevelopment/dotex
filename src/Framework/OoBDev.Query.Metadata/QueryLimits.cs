using System.Text.Json;

namespace OoBDev.Query.Metadata;

/// <summary>
/// The cost guard of a request: how many filters, how many values in one filter and how long the free text may be. The page size is clamped by the compiler.
/// </summary>
internal static class QueryLimits
{
    /// <summary>Refuses a request over the limits.</summary>
    /// <param name="request">The request.</param>
    /// <param name="options">The limits.</param>
    /// <exception cref="QueryException">A limit is exceeded (400).</exception>
    public static void Check(QueryRequest request, QueryMetadataOptions options)
    {
        if (request.Search is { } search && search.Length > options.MaxSearchLength)
        {
            throw new QueryException($"The search text is {search.Length} characters; at most {options.MaxSearchLength} are allowed.");
        }

        if (request.Filters is not { } filters)
        {
            return;
        }

        if (filters.Count > options.MaxFilters)
        {
            throw new QueryException($"The request has {filters.Count} filters; at most {options.MaxFilters} are allowed.");
        }

        foreach (var (name, value) in filters)
        {
            if (value.ValueKind == JsonValueKind.Array && value.GetArrayLength() > options.MaxFilterValues)
            {
                throw new QueryException($"The filter '{name}' lists {value.GetArrayLength()} values; at most {options.MaxFilterValues} are allowed.");
            }
        }
    }
}
