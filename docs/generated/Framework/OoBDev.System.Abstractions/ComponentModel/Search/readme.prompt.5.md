Create a readme file describing the general functionality over the follow files.

Generate a summary that includes a description of the related functionality.
In a technical summary define any design patterns or achitectural patterns described by the files.
Generate a component diagrams using plantuml.
PlantUML blocks must all be identified with "```plantuml" and closed with "```"

## Source Files

```NotSearchableAttribute.cs
using System;

namespace OoBDev.System.ComponentModel.Search;

/// <summary>
/// explicitly exclude properties from search
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public class NotSearchableAttribute : Attribute
{
    /// <summary>
    /// explicitly exclude properties from search
    /// </summary>
    /// <param name="targetName">virtual property to target</param>
    public NotSearchableAttribute(string targetName) => TargetName = targetName;

    /// <summary>
    /// explicitly exclude properties from search
    /// </summary>
    public NotSearchableAttribute() { }

    /// <summary>
    /// Target name required only if this is used on the class
    /// </summary>
    public string? TargetName { get; }
}

```

```NotSortableAttribute.cs
using System;

namespace OoBDev.System.ComponentModel.Search;

/// <summary>
/// Specifies that a property or class should not be sortable.
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public class NotSortableAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NotSortableAttribute"/> class.
    /// </summary>
    /// <param name="targetName">The name of the target property that should not be sortable.</param>
    public NotSortableAttribute(string targetName) => TargetName = targetName;

    /// <summary>
    /// Initializes a new instance of the <see cref="NotSortableAttribute"/> class.
    /// </summary>
    public NotSortableAttribute() { }

    /// <summary>
    /// Gets the name of the target property that should not be sortable.
    /// </summary>
    public string? TargetName { get; }
}

```

```SearchableAttribute.cs
using System;

namespace OoBDev.System.ComponentModel.Search;

/// <summary>
/// include field to be searchable for "SearchTerm"
/// </summary>
[AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
public class SearchableAttribute : Attribute
{
    /// <summary>
    /// mark a virtual property as searchable
    /// </summary>
    /// <param name="targetName"></param>
    public SearchableAttribute(string targetName) => TargetName = targetName;

    /// <summary>
    /// mark a property as searchable
    /// </summary>
    public SearchableAttribute() { }

    /// <summary>
    /// Target name required only if this is used on the class
    /// </summary>
    public string? TargetName { get; }
}

```

```SearchTermDefaultAttribute.cs
using OoBDev.System.Linq.Search;
using System;

namespace OoBDev.System.ComponentModel.Search;

/// <summary>
/// provide the ability to control how search terms are handled if not wilded carded
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class SearchTermDefaultAttribute(SearchTermDefaults @default) : Attribute, ISearchQueryIntercept
{
    /// <summary>
    /// rule to use for provided search term if not quoted
    /// </summary>
    public SearchTermDefaults Default { get; } = @default;

    /// <summary>
    /// use the `Default` to control pattern for searches without provided wild cards
    /// </summary>
    /// <param name="searchQuery"></param>
    /// <returns></returns>
    public ISearchQuery Intercept(ISearchQuery searchQuery)
    {
        if (searchQuery is SearchQuery query &&
            query.SearchTerm != null
            )
        {
            var searchTerm = query.SearchTerm;
            if (searchTerm.StartsWith('"') && searchTerm.EndsWith('"'))
            {
                return query with { SearchTerm = searchTerm[1..^1] };
            }

            if (!searchTerm.Contains('*'))
            {
                return query with
                {
                    SearchTerm = Default switch
                    {
                        SearchTermDefaults.StartsWith => searchTerm + "*",
                        SearchTermDefaults.EndsWith => "*" + searchTerm,
                        SearchTermDefaults.Contains => "*" + searchTerm + "*",
                        _ => searchTerm,
                    }
                };
            }
        }
        return searchQuery;
    }
}

```

```SearchTermDefaults.cs
namespace OoBDev.System.ComponentModel.Search;

/// <summary>
/// Specifies default search term options for comparison.
/// </summary>
public enum SearchTermDefaults
{
    /// <summary>
    /// Represents an equal comparison for search terms.
    /// </summary>
    EqualTo,

    /// <summary>
    /// Represents a contains comparison for search terms.
    /// </summary>
    Contains,

    /// <summary>
    /// Represents a starts-with comparison for search terms.
    /// </summary>
    StartsWith,

    /// <summary>
    /// Represents an ends-with comparison for search terms.
    /// </summary>
    EndsWith,
}

```

