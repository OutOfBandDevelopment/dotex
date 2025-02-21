# OoBDev.System.Text - SerializerTypes

## Overview

The `OoBDev.System.Text` namespace contains a single enum class called `SerializerTypes`, which specifies different types of serialization formats.

## Class Diagram

```plantuml
@startuml
class SerializerTypes {
  - enum type
  - Json
  - Bson
  - Xml
}
@enduml
```

## Description

The `SerializerTypes` enum class defines three constants that represent different serialization formats:

* `Json`: Represents the JSON serialization format.
* `Bson`: Represents the BSON (Binary JSON) serialization format.
* `Xml`: Represents the XML serialization format.

## Usage

The `SerializerTypes` enum class can be used in code to specify the desired serialization format. For example:
```csharp
SerializerTypes serializerType = SerializerTypes.Json;
```
## Class Diagram (Expanded)

```plantuml
@startuml
class SerializerTypes {
  - enum type
  - Json
  - Bson
  - Xml
}
@enduml
```

Note: The expanded class diagram shows the explicit relationships between the `SerializerTypes` enum and its underlying data types.