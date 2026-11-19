# OoBDev.System.Linq

Turns a serializable search model (filters, sort, paging) into LINQ expressions that run against any `IQueryable<T>`, so one request shape queries EF Core, in-memory lists or MongoDB. In ASP.NET Core it is applied automatically to actions that return `IQueryable<T>` ([OoBDev.AspNetCore.Mvc](../OoBDev.AspNetCore.Mvc/README.AspNetCore.Mvc.md)).

## Use

```csharp
services.TryAddSearchQueryExtensions();                      // or pass a StringCasing (Upper, Lower)

var result = queryBuilder.ExecuteBy(customers.AsQueryable(), searchQuery);   // IQueryBuilder<Customer>
```

`QueryBuilder` also has static `Execute` methods for use without dependency injection (default page size 10).

The registration adds transient `IQueryBuilder<>`, `ISortBuilder<>` and `IExpressionTreeBuilder<>`, a scoped `ICaptureResultMessage`, and the post-build expression visitors: `StringComparisonReplacementExpressionVisitor` always, and `StringOrderReplacementExpressionVisitor` when a casing other than `Default` is chosen (it forces upper or lower case on string comparisons and ordering).

## Filter operators

`EqualTo`, `NotEqualTo`, `InSet`, `LessThan`, `LessThanOrEqualTo`, `GreaterThan`, `GreaterThanOrEqualTo` (enum `Operators`). Filtering on a member excludes rows where it is null (known issue); recursive `OR`/`AND` groups are a suggested feature. `InSet` accepts any sequence (array, `List<T>`) and a whole-number text such as `0.0` matches an integer member. String values take the wildcards `*x`, `x*` and `*x*` (this is the "like" of the filter) and a leading `!` negates. The request and result shapes are in [SearchQueryMiddleware.md](SearchQueryMiddleware.md) and the examples in `notes/`.

## Notes

- [Concept and design](Design.DynamicSearchFilter.md)
- [Usage and functionality](SearchQueryMiddleware.md)
- HTTP `QUERY`, OData and GraphQL are being evaluated beside this syntax without changing it (rule `owner-decisions`).
