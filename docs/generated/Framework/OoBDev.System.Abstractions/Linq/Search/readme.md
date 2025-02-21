**README File**

The OoBDev.System.Linq.Search namespace provides a set of interfaces and classes for building and executing search queries. The main components are:

1. `FilterParameter`: A class that represents a filter parameter for a search query, allowing for filtering by equality, inequality, range, and set membership.
2. `IFilterQuery`: An interface that provides a collection of filter parameters for a search query.
3. `IPageQuery`: An interface that provides pagination options for a search query, including current page number, page size, and exclusion of total page count.
4. `IQueryBuilder`: An interface that provides methods for executing queries with search parameters, including typed and non-typed queries.
5. `ISearchQuery`: An interface that combines page, sort, search term, and filter criteria for searching data.

**Technical Summary**

The design patterns used in this namespace are:

1. The `FilterParameter` class uses the Builder design pattern to construct filter parameters.
2. The `IQueryBuilder` interface uses the Facade design pattern to simplify the execution of queries.
3. The `ISearchQuery` interface uses the Composite design pattern to combine page, sort, search term, and filter criteria.

The architectural patterns used are:

1. The Repository pattern is used to decouple the query builder from the data source.
2. The Interface Segregation Principle (ISP) is used to separate the concerns of different query types (e.g., typed and non-typed).

**Component Diagram**

```plantuml
@startuml
!include https://raw.githubusercontent.com/plantuml-stdlib/C4-Model/master/c4company.puml

!include https://raw.githubusercontent.com/plantuml-stdlib/C4-Model/master/c4system.puml

!include https://raw.githubusercontent.com/plantuml-stdlib/C4-Model/master/c4component.puml

component "OoBDev.System.Linq.Search" {
    boundary "Search Query Builder" as QB
    component "IQueryBuilder" as QB_Interface
    boundary "Filter Parameters" as FP
    component "FilterParameter" as FP_Component
    boundary "Search Query" as SQ
    component "ISearchQuery" as SQ_Interface
    boundary "Page and Sort" as PS
    component "IPageQuery" as PS_Interface
    boundary "Search Term" as ST
    "ISearchTermQuery" as ST_Interface
}

QB_Address ->> QB_Interface : ExecuteBy
FP_Address ->> FP_Component : FilterParameter
SQ_Address ->> SQ_Interface : ISearchQuery
PS_Address ->> PS_Interface : IPageQuery
ST_Address ->> ST_Interface : ISearchTermQuery

note "Search Query Builder uses Filter Parameters to filter data"
note "Search Query combines Page and Sort with Search Term and Filter Parameters to define search criteria"
@enduml
```

Note: The above `plantuml` code generates a component diagram showing the relationships between the main components in the OoBDev.System.Linq.Search namespace.