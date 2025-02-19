Create a readme file describing the general functionality over the follow files.

Generate a summary that includes a description of the related functionality.
In a technical summary define any design patterns or achitectural patterns described by the files.
Generate a component diagrams using plantuml.
PlantUML blocks must all be identified with "```plantuml" and closed with "```"

## Source Files

```SearchQuery.cs
using System.Collections.Generic;
using System.Text;

namespace OoBDev.System.Linq.Search;

/// <summary>
/// Represents a search query with filtering and sorting options for a specific model.
/// </summary>
/// <typeparam name="TModel">The type of the model.</typeparam>
public record SearchQuery<TModel> : SearchQuery, ISearchQuery<TModel>
{
}

/// <summary>
/// Represents a generic search query with filtering and sorting options.
/// </summary>
public record SearchQuery : ISearchQuery
{
    internal static class Messages
    {
        public const string NoSearchQueryFilter = $"No filtering detected";
        public const string NoSearchQueryFilterCode = "NO_SEARCHQUERY_FILTER";

        public const string NoPropertyFilter = $"No filtering for property";
        public const string NoPropertyFilterCode = "NO_PROPERTY_FILTER";

        public const string SortPropertiesNotFound = $"Sort properties not found on model";
        public const string SortPropertiesNotFoundCode = "SORT_PROPERTY_NOT_FOUND";

        public const string ApplySortDefault = $"Applying Default Sort Order";
        public const string ApplySortDefaultCode = "SORT_DEFAULT_APPLIED";

        public const string ForceSortDefault = $"Forcing Sort Order";
        public const string ForceSortDefaultCode = "SORT_DEFAULT_FORCED";
    }

    /// <summary>
    /// Gets or sets the current page number.
    /// </summary>
    public int CurrentPage { get; set; }

    /// <summary>
    /// Gets or sets the page size.
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to exclude page count from the result.
    /// </summary>
    public bool ExcludePageCount { get; set; }

    /// <summary>
    /// Gets or sets the search term.
    /// </summary>
    public string? SearchTerm { get; set; }

    /// <summary>
    /// Gets or initializes the filters for the query.
    /// </summary>
    public Dictionary<string, FilterParameter> Filter { get; init; } = [];

    /// <summary>
    /// Gets or initializes the sorting options for the query.
    /// </summary>
    public Dictionary<string, OrderDirections> OrderBy { get; init; } = [];

    /// <inheritdoc cref="IFilterQuery.Filter"/>
    IDictionary<string, FilterParameter> IFilterQuery.Filter => Filter;

    /// <inheritdoc cref="ISortQuery.OrderBy"/>
    IDictionary<string, OrderDirections> ISortQuery.OrderBy => OrderBy;

    /// <summary>
    /// Generates a string representation of the search query.
    /// </summary>
    /// <returns>A string representation of the search query.</returns>
    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"{nameof(CurrentPage)}: {CurrentPage}");
        sb.AppendLine($"{nameof(PageSize)}: {PageSize}");
        sb.AppendLine($"{nameof(ExcludePageCount)}: {ExcludePageCount}");
        sb.AppendLine($"{nameof(SearchTerm)}: {SearchTerm ?? "(null)"}");

        foreach (var item in Filter ?? [])
        {
            sb.AppendLine($"\tF:{item.Key}: {item.Value}");
        }
        foreach (var item in OrderBy ?? [])
        {
            sb.AppendLine($"\tS:{item.Key}: {item.Value}");
        }
        return sb.ToString();
    }
}

```

