**README File**

**Summary**

The provided source files constitute a search framework designed to facilitate search operations on entities. The framework allows for customizations such as default sorting, filtering, and ignores specific properties for string comparison. It provides an interface for intercepting and modifying search queries.

**Technical Summary**

The search framework employs several design patterns and architectural patterns:

* **Attribute-based programming**: The framework uses attributes (e.g., `DefaultSortAttribute`, `FilterableAttribute`, `NotFilterableAttribute`) to decorate entities and their properties, enabling customization and configuration of search operations.
* **Single Responsibility Principle (SRP)**: Each attribute has a single responsibility, making it modular and easy to maintain.
* **Dependency Injection**: The framework utilizes interface-based programming (e.g., `ISearchQueryIntercept`) to decouple components and enable testability.

**Component Diagram**

```plantuml
@startuml
class DefaultSortAttribute {
  - TargetName: string?
  - Priority: int
  - Order: OrderDirections
}
class FilterableAttribute {
  - TargetName: string?
}
class IgnoreStringComparisonReplacementAttribute {
}
class ISearchQueryIntercept {
  + Intercept (ISearchQuery)
}
class NotFilterableAttribute {
  - TargetName: string?
}

defaultSortAttribute <- left -- right in DefaultSortAttribute
filterableAttribute <- left -- right in FilterableAttribute
ignoreStringComparisonReplacementAttribute <- left -- right in IgnoreStringComparisonReplacementAttribute
isSearchQueryIntercept <- left -- right in ISearchQueryIntercept
notFilterableAttribute <- left -- right in NotFilterableAttribute
@enduml
```

This diagram illustrates the relationships between the attributes and the interface. The attributes are interconnected, and the interface has a dependency on the attributes.