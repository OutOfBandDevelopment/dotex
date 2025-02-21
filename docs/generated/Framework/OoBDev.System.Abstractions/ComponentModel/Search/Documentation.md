**Search Attribute Documentation**
=============================

**Overview**
-----------

This documentation provides an overview of the search attributes used in the OoBDev System Composition library. These attributes provide a way to customize the search functionality by specifying which properties or classes can be used for sorting, filtering, and other search operations.

**Attributes**
-------------

### DefaultSortAttribute

The `DefaultSortAttribute` is used to specify the default sort order for an entity. This attribute can be applied to properties or classes.

**Constructors**

* `DefaultSortAttribute(string? targetName = default, int priority = default, OrderDirections order = OrderDirections.Ascending)`: Initializes a new instance of the `DefaultSortAttribute` class with the specified `targetName`, `priority`, and `order`.
* `DefaultSortAttribute()`: Initializes a new instance of the `DefaultSortAttribute` class with default values.

**Properties**

* `TargetName`: Gets the property name to use for mapping.
* `Priority`: Gets the sort column position priority.
* `Order`: Gets the direction to order this mapped column.

### FilterableAttribute

The `FilterableAttribute` is used to specify which properties or classes can be used for filtering.

**Constructors**

* `FilterableAttribute(string targetName)`: Initializes a new instance of the `FilterableAttribute` class with the specified `targetName`.
* `FilterableAttribute()`: Initializes a new instance of the `FilterableAttribute` class with a default target name.

**Properties**

* `TargetName`: Gets the column mapping override.

### IgnoreStringComparisonReplacementAttribute

The `IgnoreStringComparisonReplacementAttribute` is used to exclude properties from string comparison replacement visitors.

**Constructors**

* No constructors.

### ISearchQueryIntercept

The `ISearchQueryIntercept` interface provides a way to intercept and override search queries.

**Methods**

* `Intercept(ISearchQuery searchQuery)`: Modifies or passes through the search query before processing.

### NotFilterableAttribute

The `NotFilterableAttribute` is used to specify which properties or classes should be explicitly excluded from filter selection.

**Constructors**

* `NotFilterableAttribute(string targetName)`: Initializes a new instance of the `NotFilterableAttribute` class with the specified `targetName`.
* `NotFilterableAttribute()`: Initializes a new instance of the `NotFilterableAttribute` class with a default target name.

**Properties**

* `TargetName`: Gets the target name to be excluded from filter selection.

**Class Diagram**
```
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
  - No properties
}

interface ISearchQueryIntercept {
  - intercept(ISearchQuery searchQuery): ISearchQuery
}

class NotFilterableAttribute {
  - TargetName: string?
}

@enduml
`````