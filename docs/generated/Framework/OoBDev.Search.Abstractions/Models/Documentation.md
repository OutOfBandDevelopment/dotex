**Documentation for OoBDev Search**

**Overview**

The OoBDev Search system is a search engine that allows users to search for various types of data. The system is designed to provide a comprehensive search solution that can handle different types of searches.

**Search Result Model**

### Class Diagram

````plantuml
@startuml
class SearchResultModel {
  - score: float (required)
  - item_id: string (required)
  - meta_data: Dictionary<string, object> (optional)
}

@enduml
```


**Description**

The `SearchResultModel` class represents a search result model containing information about a search result. It has three properties:

* `Score`: The score of the search result. This property is required.
* `ItemId`: The item's id value. This property is required.
* `MetaData`: The item's metadata. This property is optional and can be null.

**SearchTypes**

### Enum Diagram

````plantuml
@startuml
enum SearchTypes {
  None
  Semantic = 1
  Lexical = 2
  Hybrid = 3
}
@enduml
```


**Description**

The `SearchTypes` enum specifies the types of search. It has four values:

* `None`: Indicates no specific search type.
* `Semantic`: Indicates a semantic search type.
* `Lexical`: Indicates a lexical search type.
* `Hybrid`: Indicates a hybrid search type combining semantic and lexical searches.

**Sequence Diagram**

````plantuml
@startuml
participant User
participant OoBDevSearch
note "User searches for data"
User -> OoBDevSearch: search query
OoBDevSearch -> User: search results
@enduml
```


**Description**

The sequence diagram shows the interaction between the user and the OoBDev Search system. The user sends a search query to the system, and the system returns the search results.

Note: This is a simple sequence diagram and can be extended to show more details about the search process.