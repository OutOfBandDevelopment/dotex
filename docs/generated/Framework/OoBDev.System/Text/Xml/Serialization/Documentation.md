# OoBDev.System.Text.Xml.Serialization

## DefaultXmlSerializer

### Overview

`DefaultXmlSerializer` is a basic XML serialization implementation that provides functionality for converting objects into and out of XML format.

### Class Diagram

```plantuml
@startuml
class IXmlSerializer {
  - serialize(object obj, Type type)
  - serialize(T obj)
  - deserialize(T? obj, Stream stream)
  - deserialize(object? obj, Type type, Stream stream)
  - deserializeAsync(T? obj, Stream stream, CancellationToken cancellationToken)
  - deserializeAsync(object? obj, Type type, Stream stream, CancellationToken cancellationToken)
}

class DefaultXmlSerializer {
  - serialize(object obj, Type type)
  - serialize(T obj)
  - deserialize(T? obj, Stream stream)
  - deserialize(object? obj, Type type, Stream stream)
  - deserializeAsync(T? obj, Stream stream, CancellationToken cancellationToken)
  - deserializeAsync(object? obj, Type type, Stream stream, CancellationToken cancellationToken)

  + Deserialize<T>(Stream stream)
  + Deserialize(object? obj, Type type)
  + Serialize(object? obj, Type type)
  + Serialize<T>(T obj)
}
@enduml
```