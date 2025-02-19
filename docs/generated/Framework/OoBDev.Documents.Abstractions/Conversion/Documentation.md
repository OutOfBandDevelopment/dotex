# IDocumentConversionHandler Documentation

## Overview

The `IDocumentConversionHandler` interface represents a handler for document conversion. It defines a set of methods that a concrete implementation must provide to support document conversion.

## Interface Definition

### Sources

The `Sources` property returns an array of supported source content types for document conversion.

``` plantuml
@startuml
interface IDocumentConversionHandler {
  - Sources: string[]
}
@enduml
```

### SupportedSource

The `SupportedSource` method determines whether a specified content type is supported as a source for document conversion.

``` plantuml
@startuml
interface IDocumentConversionHandler {
  bool SupportedSource(string contentType)
}
@enduml
```

### Destinations

The `Destinations` property returns an array of supported destination content types for document conversion.

``` plantuml
@startuml
interface IDocumentConversionHandler {
  - Destinations: string[]
}
@enduml
```

### SupportedDestination

The `SupportedDestination` method determines whether a specified content type is supported as a destination for document conversion.

``` plantuml
@startuml
interface IDocumentConversionHandler {
  bool SupportedDestination(string contentType)
}
@enduml
```

## Sequence Diagram

The following sequence diagram illustrates the interaction between a client and an `IDocumentConversionHandler` implementation.

``` plantuml
@startuml
sequenceDiagram
(participant "Client" as c)
(participant "Handler" as h)

c->>h: GetSources
h->>c: [Sources]
c->>h: IsSupportedSource(contentType)
h->>c: [booleanResult]
c->>h: GetDestinations
h->>c: [Destinations]
c->>h: IsSupportedDestination(contentType)
h->>c: [booleanResult]
@enduml
```

## UML Class Diagram

The following class diagram illustrates the relationship between `IDocumentConversionHandler` and its constituent parts.

``` plantuml
@startuml
class IDocumentConversionHandler {
    - Sources: string[]
    - Destinations: string[]
    + SupportedSource(string): boolean
    + SupportedDestination(string): boolean
}
@enduml
```

Note: This documentation assumes that `IDocumentConversion` is an interface or an abstract class that defines the base methods for document conversion handlers. If this is not the case, you may need to modify the sequence diagram and UML class diagram accordingly.