# Default BSON and JSON Serializers Documentation

## Introduction

The `DefaultBsonSerializer` and `DefaultJsonSerializer` classes are part of the OoBDev System.Text.Json.Serialization namespace. They provide default serialization and deserialization for BSON (Binary JSON) and JSON data formats.

## Class Diagrams

```plantuml
@startuml
class DefaultBsonSerializer {
  -+ DefaultContentType : string
  -+ ContentType : string
  -+ _options : JsonSerializerOptions
}

class DefaultJsonSerializer {
  -+ DefaultOptions : JsonSerializerOptions
  -+ DefaultContentType : string
  -+ ContentType : string
  -+ _options : JsonSerializerOptions
}

class IBsonSerializer {
  + Serialize(T obj)
  + Deserialize(Stream stream)
}

class IJsonSerializer {
  + Serialize(T obj)
  + Deserialize(Stream stream)
}

@enduml
```

## Component Model

```plantuml
@startuml
component DefaultBsonSerializer {
  note "BSON serialization and deserialization"
  rectangle "BsonTypeInfoResolver" {
    note "Resolves type information for BSON serialization"
  }
}

component DefaultJsonSerializer {
  note "JSON serialization and deserialization"
  rectangle "JsonSerializerOptions" {
    note "Configures JSON serialization options"
  }
}

component JsonSerializer {
  note "Serializes and deserializes JSON data"
}

@enduml
```

## Sequence Diagrams

### DefaultBsonSerializer

```plantuml
@startuml
actor "User" as user
participant "DefaultBsonSerializer" as serializer
participant "BSON data" as data

user ->> serializer: Serialize(obj)
serializer -> data: Serialize(obj) -> JSON string
serializer ->> user: JSON string

user ->> serializer: Deserialize(stream)
serializer ->> data: Deserialize(stream) -> BSON data
serializer ->> user: BSON data

@enduml
```

### DefaultJsonSerializer

```plantuml
@startuml
actor "User" as user
participant "DefaultJsonSerializer" as serializer
participant "JSON data" as data

user ->> serializer: Serialize(obj)
serializer ->> data: Serialize(obj) -> JSON string
serializer ->> user: JSON string

user ->> serializer: Deserialize(stream)
serializer ->> data: Deserialize(stream) -> JSON data
serializer ->> user: JSON data

user ->> serializer: Deserialize(input)
serializer ->> data: Deserialize(input) -> JSON data
serializer ->> user: JSON data

@enduml
```

## Code Documentation

### DefaultBsonSerializer.cs

The `DefaultBsonSerializer` class is responsible for serializing and deserializing BSON data. It is a part of the OoBDev System.Text.Json.Serialization namespace.

```csharp
namespace OoBDev.System.Text.Json.Serialization;

public class DefaultBsonSerializer : DefaultJsonSerializer, IBsonSerializer
{
    // ...
}
```

### DefaultJsonSerializer.cs

The `DefaultJsonSerializer` class is responsible for serializing and deserializing JSON data. It is a part of the OoBDev System.Text.Json.Serialization namespace.

```csharp
namespace OoBDev.System.Text.Json.Serialization;

public class DefaultJsonSerializer : IJsonSerializer
{
    // ...
}
```

I hope this documentation meets your requirements! Let me know if you have any further requests.