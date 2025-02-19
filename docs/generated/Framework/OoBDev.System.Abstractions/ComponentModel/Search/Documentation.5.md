# Documentation for OoBDev.System.ComponentModel.Search

## Introduction

This documentation provides an overview of the OoBDev.System.ComponentModel.Search namespace, which provides attributes for controlling search behavior in .NET applications.

## Attributes

### NotSearchableAttribute

The `NotSearchableAttribute` is used to explicitly exclude properties or classes from search results.

```plantuml
@startuml
class NotSearchableAttribute {
  -targetName: string?
  +TargetName
  -TargetName(?)
}
@enduml
```

### NotSortableAttribute

The `NotSortableAttribute` is used to specify that a property or class should not be sortable.

```plantuml
@startuml
class NotSortableAttribute {
  -targetName: string?
  +TargetName
  -TargetName(?)
}
@enduml
```

### SearchableAttribute

The `SearchableAttribute` is used to mark a property or class as searchable.

```plantuml
@startuml
class SearchableAttribute {
  -targetName: string?
  +TargetName
  -TargetName(?)
}
@enduml
```

### SearchTermDefaultAttribute

The `SearchTermDefaultAttribute` is used to specify default search term options for comparison.

```plantuml
@startuml
class SearchTermDefaultAttribute {
  -default: SearchTermDefaults
  +Default
  -ISearchQuery Intercept(ISearchQuery searchQuery)
}
@enduml
```

### SearchTermDefaults

The `SearchTermDefaults` enum specifies default search term options for comparison.

```plantuml
@startuml
enum SearchTermDefaults {
  StartsWith
  EndsWith
  Contains
  EqualTo
}
@enduml
```

## Sequence Diagram

Here is a sequence diagram showing the interaction between the `SearchTermDefaultAttribute` and the `ISearchQuery` interface:
```plantuml
@startuml
actor "Search Query" as sq
participant "SearchTermDefaultAttribute" as sda
note "default search term option selected" as n
sq -> sda: Invoke Intercept
sda -> sq: Intercept(ISearchQuery searchQuery)
note "search query processed" as m
@enduml
```

This diagram shows how the `SearchTermDefaultAttribute` intercepts the search query and applies the default search term option specified in the attribute.

## Component Model

Here is a high-level component model showing the relationships between the different components:
```plantuml
@startuml
class System (<<system>>) {
  -[r:System]
  System -> OoBDev.System.ComponentModel.Search (<<search›)
  OoBDev.System.ComponentModel.Search -> NotSearchableAttribute (<<attribute›)
  OoBDev.System.ComponentModel.Search -> NotSortableAttribute (<<attribute›)
  OoBDev.System.ComponentModel.Search -> SearchableAttribute (<<attribute›)
  OoBDev.System.ComponentModel.Search -> SearchTermDefaultAttribute (<<attribute›)
  OoBDev.System.ComponentModel.Search -> SearchTermDefaults (<<enum›)
}
@enduml
```

This component model shows the OoBDev.System.ComponentModel.Search namespace as a component that provides a set of attributes for controlling search behavior. The attributes are used to customize the search behavior of properties and classes in the system.