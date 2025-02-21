As a senior software engineer/solutions architect, I'll review the provided source code and suggest changes to improve maintainability, scalability, and readability.

**1. Naming Conventions:**

a. The class name `SearchQuery` starts with a capital letter, but the property and method names do not. To maintain consistency, I recommend using PascalCase for all class, property, and method names.
b. The `IFilterQuery` and `ISortQuery` interfaces should have names that are more descriptive and follow the same naming pattern.

**2. Code Organization:**

a. Separate concerns by moving the `SearchQuery` class into a separate namespace, e.g., `OoBDev.System.Linq.Paging`.
b. Move the `Messages` class to a separate file or rename it to a more descriptive name, as it's not related to the `SearchQuery` class.

**3. code comments:**

a. Remove unnecessary comments and focus on providing meaningful and clear descriptions for methods, properties, and classes.
b. Consider using XML comments for public methods and properties to enable intellisense and other tools.

**4. Performance Optimization:**

a. Avoid creating unnecessary temporary lists (e.g., `Filter` and `OrderBy`) in the `ToString()` method. Instead, iterate directly over the dictionary values.
b. Consider implementing the `ToString()` method using a more efficient concatenation approach, such as using a `StringBuilder`.

**5. Code Readability and Maintainability:**

a. Split long lines of code into shorter, more readable lines.
b. Consistent indentation and spacing
c. Consider changing the `init` keyword to `new` for the `Filter` and `OrderBy` dictionaries, as it's more readable and understandable.
d. Avoid using `?? []` when initializing dictionaries. Instead, use the dictionary constructor or `new Dictionary<string, FilterParameter>()`.

Here's an updated version of the code:

```csharp
using System.Collections.Generic;
using System.Text;

namespace OoBDev.System.Linq.Paging;

public record SearchQuery<TModel> : SearchQuery, ISearchQuery<TModel>
{
    // ...
}

public record SearchQuery : ISearchQuery
{
    private const string NoSearchQueryFilter = "No filtering detected";
    private const string NoSearchQueryFilterCode = "NO_SEARCHQUERY_FILTER";
    // ...

    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public bool ExcludePageCount { get; set; }
    public string? SearchTerm { get; set; }

    public Dictionary<string, FilterParameter> Filter { get; } = new();
    public Dictionary<string, OrderDirections> OrderBy { get; } = new();

    IDictionary<string, FilterParameter> IFilterQuery.Filter => Filter;
    IDictionary<string, OrderDirections> ISortQuery.OrderBy => OrderBy;

    public override string ToString()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"CurrentPage: {CurrentPage}");
        sb.AppendLine($"PageSize: {PageSize}");
        sb.AppendLine($"ExcludePageCount: {ExcludePageCount}");
        sb.AppendLine($"SearchTerm: {SearchTerm ?? "(null)"}");

        foreach (var (key, value) in Filter)
        {
            sb.AppendLine($"F:{key}: {value}");
        }
        foreach (var (key, value) in OrderBy)
        {
            sb.AppendLine($"S:{key}: {value}");
        }
        return sb.ToString();
    }
}
```

These changes aim to improve code readability, maintainability, and performance. However, please note that a more comprehensive review and refactoring may be necessary to fully optimize the code for the specific requirements of your project.