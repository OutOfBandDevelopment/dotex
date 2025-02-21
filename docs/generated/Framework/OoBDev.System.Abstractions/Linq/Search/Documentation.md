Here is the documentation for the source code in Markdown format:

# OoBDev System Linq Search

## Overview

The OoBDev System Linq Search library provides a set of classes and interfaces for building and executing queries with search parameters. The library includes classes for filtering, sorting, and paginating data, as well as converting filter parameters to strings.

## Classes

### FilterParameter

```plantuml
@startuml
class FilterParameter {
  - EqualTo: object?
  - NotEqualTo: object?
  - InSet: object[]?
  - GreaterThan: object?
  - GreaterThanOrEqualTo: object?
  - LessThan: object?
  - LessThanOrEqualTo: object?
}

@enduml
```

The `FilterParameter` class represents a filter parameter with various properties for filtering data. The properties include `EqualTo`, `NotEqualTo`, `InSet`, `GreaterThan`, `GreaterThanOrEqualTo`, `LessThan`, and `LessThanOrEqualTo`.

### IFilterQuery

```plantuml
@startuml
interface IFilterQuery {
  - Filter: IDictionary<string, FilterParameter>?
}

@enduml
```

The `IFilterQuery` interface represents a query with filtering options. The `Filter` property returns a dictionary of filter parameters.

### IPageQuery

```plantuml
@startuml
interface IPageQuery {
  - CurrentPage: int
  - PageSize: int
  - ExcludePageCount: bool
}

@enduml
```

The `IPageQuery` interface represents a page query for paginating results. The `CurrentPage`, `PageSize`, and `ExcludePageCount` properties provide information about the current page and the size of each page.

### IQueryBuilder

```plantuml
@startuml
interface IQueryBuilder {
  - ExecuteBy(query: IQueryable, searchQuery: ISearchQuery): IQueryResult
}

interface IQueryBuilder<TModel> {
  - ExecuteBy(query: IQueryable<TModel>, searchQuery: ISearchQuery): IQueryResult<TModel>
}

@enduml
```

The `IQueryBuilder` interface represents a query builder for executing queries with search parameters. There are two versions of the interface: a generic version that can be used with any type, and a typed version that is used with a specific type.

### ISearchQuery

```plantuml
@startuml
interface ISearchQuery {
  - PageQuery: IPageQuery
  - SortQuery: ISortQuery
  - SearchTermQuery: ISearchTermQuery
  - FilterQuery: IFilterQuery
}

interface ISearchQuery<TModel> {
}

@enduml
```

The `ISearchQuery` interface represents a query object that combines page, sort, search term, and filter criteria for searching data. The generic version of the interface can be used with any type, and the typed version is used with a specific type.

## Sequence Diagram

```plantuml
@startuml
sequenceDiagram
    participant QueryBuilder as "QueryBuilder"
    participant QuerySample as "QuerySample"
    participant SearchQuery as "SearchQuery"

    QueryBuilder->>QuerySample: Retrieve data
    QueryBuilder->>SearchQuery: Get search query
    SearchQuery->>QueryBuilder: Execute query
    QueryBuilder->>QuerySample: Retrieve filtered data

    note overwrite: "The QueryBuilder uses the SearchQuery to filter the data."
@enduml
```

This sequence diagram shows how the QueryBuilder uses the SearchQuery to filter data. The QueryBuilder retrieves data from a sample query, gets the search query, executes the query, and retrieves the filtered data.