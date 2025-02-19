**README File**

This repository contains the implementation of a search query system, designed to provide a flexible and robust way to search and filter data. The system is built around the `SearchQuery` class, which represents a generic search query with filtering and sorting options.

**Summary**

The `SearchQuery` class allows you to specify filtering and sorting options for a specific model. It provides a simple and intuitive way to define search queries, allowing you to filter and sort results based on specific criteria. The system also includes support for pagination, allowing you to retrieve results in pages.

**Technical Summary**

The `SearchQuery` class uses the following design patterns and architectural patterns:

* **Repository Pattern**: The `SearchQuery` class acts as a repository, providing a unified interface for searching and filtering data.
* **Strategy Pattern**: The filtering and sorting options are implemented using strategy patterns, allowing you to add or remove filters and sorters without modifying the core logic of the class.
* **Memento Pattern**: The `SearchQuery` class uses the memento pattern to store the current state of the query, allowing you to persist the query and restore it later.

**Component Diagram**

```plantuml
@startuml
class SearchQuery {
  - pageSize: int
  - currentPage: int
  - excludePageCount: bool
  - searchTerm: string?
  - filter: Dictionary<string, FilterParameter>
  - orderBy: Dictionary<string, OrderDirections>
  +toString(): string
}

class FilterParameter {
  - key: string
  - value: string
}

class OrderDirections {
  - key: string
  - value: OrderDirection
}

class OrderDirection {
  - asc: bool
  - desc: bool
}

SearchQuery --+> FilterParameter
SearchQuery --+> OrderDirections
FilterParameter --+> OrderDirection

@enduml
```

This component diagram shows the relationships between the classes and interfaces in the system. The `SearchQuery` class is the main class, and it has associations with the `FilterParameter` and `OrderDirections` classes. The `FilterParameter` class has a nested class `OrderDirection`, which represents the possible order directions for the filter.