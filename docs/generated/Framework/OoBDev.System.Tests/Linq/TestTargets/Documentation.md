# Documentation for OoBDev.System.Tests.Linq.TestTargets

## Overview

This project contains test models for the OoBDev System, a LINQ-based search system. The models are used to test the search functionality and other features of the system.

## Classes

### TestTarget2Model

```plantuml
@startuml
class TestTarget2Model {
    - int Index [key, not sortable]
    - string Name [searchable]
    - string Email [not filterable]
}
@enduml
```

The TestTarget2Model class represents a basic model with a primary key, a searchable string property, and a non-filterable string property.

### TestTarget3Model

```plantuml
@startuml
class TestTarget3Model {
    - int Index [key, not searchable]
    - string Name [not sortable]
    - string Email [not filterable]
}
@enduml
```

The TestTarget3Model class represents a basic model with a primary key, but with a non-searchable property and a non-sortable property.

### TestTargetExtendedModel

```plantuml
@startuml
class TestTargetExtendedModel {
    - int Index [key, default sort]
    - string FName [required, searchable]
    - string? LName [searchable]
    - string Email [required, searchable]
    - string? May [searchable]
    - string[]? Modules [searchable]
    - string[]? NullModules [searchable]
    - DateTime Date [searchable]
    - DateTime? DateTimeNullable [searchable]
    - DateTimeOffset? DateTimeOffsetNullable [searchable]
}
@enduml
```

The TestTargetExtendedModel class represents a more advanced model with multiple searchable properties, including a required property, a nullable property, and an array property. It also includes two date and time properties with nullable types.

### TestTargetModel

```plantuml
@startuml
class TestTargetModel {
    - int Index [key]
    - string Name
    - string Email
}
@enduml
```

The TestTargetModel class represents a basic model with a primary key and two simple properties.

### TestTargetWithInnerArrayModel

```plantuml
@startuml
class TestTargetWithInnerArrayModel {
    - int Index [key]
    - string Name
    - string Email
    - string[] Children
}
@enduml
```

The TestTargetWithInnerArrayModel class represents a model with an array property that contains inner strings.