# OoBDev.System.Accessors Documentation

## Accessor Class

### Overview

The `Accessor<T>` class represents an accessor for a value of type `T`. It provides a way to get and set the value associated with the accessor.

### Class Diagram
```plantuml
@startuml
class Accessor<T> {
  - Value: T?
  + Value: T? get; set;
}
Accessor<T> implements IAccessor<T>
@enduml
```
### Component Model
```plantuml
@startuml
component Accessor {
  * Value
}
Accessor ..> IAccessor<T>
@enduml
```
### Sequence Diagram

Here is an example sequence diagram showing how the `Accessor<T>` class is used:
```plantuml
@startuml
actor User
participant Accessor
participant TargetSystem

note over Accessor "Get value"
Accessor -> TargetSystem: GetValue()
TargetSystem -> Accessor: T?
Accessor -> User: T?
note over Accessor "Set value"
User -> Accessor: T?
Accessor -> TargetSystem: SetValue(T)
note over Accessor "Get value again"
Accessor -> User: T?
@enduml
```
### Code Explanation

The `Accessor<T>` class is implemented as an internal class in the `OoBDev.Extensions.Accessors` namespace.

The class has a private readonly field `_local` of type `AsyncLocal<T?>` which is used to store the value associated with the accessor.

The `Value` property gets or sets the value associated with the accessor. This property is implemented using the getter and setter methods of the `_local` field.

The `Accessor<T>` class implements the `IAccessor<T>` interface, which defines the contract for accessing values of type `T`.

### Note

The `Accessor<T>` class is designed to work with asynchronous code, and the `_local` field uses the `AsyncLocal<T>` class from the .NET framework to store the value. This allows the accessor to be used in asynchronous methods and ensure that the value is accessed and set correctly.

It is worth noting that the `Accessor<T>` class is an internal class and is not intended to be used directly from outside the `OoBDev.Extensions.Accessors` namespace. It is intended to be used by other classes and methods within the assembly.