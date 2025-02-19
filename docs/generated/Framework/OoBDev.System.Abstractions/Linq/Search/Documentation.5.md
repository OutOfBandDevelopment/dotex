# OoBDev.System.Linq.Search Documentation

## ISearchTermQuery Interface

```plantuml
@startuml
interface "ISearchTermQuery" {
  - string? SearchTerm
}
@enduml
```

### Summary

Represents a query object that includes a search term for filtering data.

### Properties

* `SearchTerm`: Gets the search term used for filtering data.

## ISortBuilder Interface

```plantuml
@startuml
interface "ISortBuilder<TModel>" {
  - IOrderedQueryable<TModel> SortBy(QQueryable<TModel> query, ISortQuery searchRequest, IExpressionTreeBuilder<TModel> treeBuilder, StringComparison stringComparison)
}
@enduml
```

### Summary

Represents an interface for building sorting expressions and applying sorting to a query.

### Methods

* `SortBy(Queryable<TModel> query, ISortQuery searchRequest, IExpressionTreeBuilder<TModel> treeBuilder, StringComparison stringComparison)`: Sorts the specified query based on the provided sort criteria.

### Type Parameters

* `TModel`: The type of the model.

## ISortQuery Interface

```plantuml
@startuml
interface "ISortQuery" {
  - IDictionary<string, OrderDirections> OrderBy
}
@enduml
```

### Summary

Represents an interface for specifying sorting criteria in a query.

### Properties

* `OrderBy`: Gets a dictionary containing sorting information, where the key is the column name and the value is the sort direction.

## OrderDirections Enum

```plantuml
@startuml
enum "OrderDirections" {
  Ascending
  Descending
}
@enduml
```

### Summary

Enumeration to control sort order.

### Values

* `Ascending`: Sort related items in ascending order.
* `Descending`: Sort related items in descending order.

## OrderDirectionsConstants Class

```plantuml
@startuml
class "OrderDirectionsConstants" {
  + const string Ascending
  + const string AscendingShort
  + const string Descending
  + const string DescendingShort
}
@enduml
```

### Summary

Provides constants for order directions.

### Constants

* `Ascending`: Represents the ascending order direction.
* `AscendingShort`: Represents a short form of the ascending order direction.
* `Descending`: Represents the descending order direction.
* `DescendingShort`: Represents a short form of the descending order direction.