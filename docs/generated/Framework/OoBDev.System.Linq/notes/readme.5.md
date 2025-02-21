**README File**

**Summary**

This repository contains multiple JSON files that define a search query and its corresponding parameters. The `SearchByObjectId.json` file specifies a filter by object ID, while the `SearchQuery.json` file contains more comprehensive search query parameters, including filtering by multiple properties, ordering by specific fields, and pagination.

**Technical Summary**

The design patterns and architectural patterns used in these files are:

1. **Configuration files**: Each JSON file represents a configuration file for the search query, allowing for easy modification and versatility.
2. **Decoupling**: The separation of concerns between the filter, search term, and pagination parameters enables decoupling of the search query logic from other parts of the application.
3. **Flexibility**: The use of JSON objects and arrays allows for easy extension and modification of the search query parameters, making it flexible and adaptable to different use cases.

**Component Diagram**

```
```plantuml
@startuml
component "SearchQuery" {
  note "SearchQuery.json"
  reference "Filter"
  reference "SearchTerm"
  reference "ExcludePageCount"
  reference "OrderBy"
  reference "Pagination" as "page"
  note "SearchQuery.json"
}

component "Filter" {
  note "Filter Property"
  reference "Property1"
  reference "Property2"
  reference "..."
}

component "Filter Property" {
  note "="
  reference "match term"
  reference "regex"
  reference "case insensitive"
}

component "SearchTerm" {
  note "Search value"
  reference "Exact match"
  reference "Contains"
  reference "startswith"
  reference "endswith"
}

component "ExcludePageCount" {
  note "Boolean flag"
  reference "true"
  reference "false"
}

component "OrderBy" {
  note "Sort Order"
  reference "Ascending"
  reference "Descending"
}

component "Pagination" {
  note "Page Index"
  reference "CurrentPage"
  reference "PageSize"
}

SearchQuery -> Filter: Filter Properties
SearchQuery -> SearchTerm: Search Value
SearchQuery -> ExcludePageCount: Flag
SearchQuery -> OrderBy: Sort Order
SearchQuery -> Pagination: Page Index
Filter -> Filter Property: Filter Properties
Filter Property -> "=", regex, case insensitive: Filter Rules
SearchTerm -> Exact match, Contains, startswith, endswith: Search Operators
OrderBy -> Ascending, Descending: Sort Directions
Pagination -> CurrentPage, PageSize: Page Settings
@enduml
```
```