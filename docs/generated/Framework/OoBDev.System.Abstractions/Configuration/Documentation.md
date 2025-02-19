# Documentation for OoBDev.System.Configuration

## CommandParameterAttribute

### Overview

The `CommandParameterAttribute` is an attribute used to specify that a property represents a command parameter.

### Class Diagram
```plantuml
@startuml
class CommandParameterAttribute {
  - short: string?
  - value: string?
  - type: object
}
@enduml
```
### Description

The `CommandParameterAttribute` is an attribute that can be applied to a property in a class. It indicates that the property represents a command parameter.

### Properties

#### Short

* Type: `string?`
* Gets or sets the short representation of the command parameter.

#### Value

* Type: `string?`
* Gets or sets the value of the command parameter.

#### TypeId

* Type: `object`
* Gets a unique identifier for this attribute.

### Example
```csharp
public class MyCommand {
  [CommandParameter]
  public string MyParameter { get; set; }
}
```
### Sequence Diagram
```plantuml
@startuml
actor User
participant CommandExecutor
participant MyCommand

User ->> CommandExecutor: execute
CommandExecutor ->> MyCommand: getMyParameter
MyCommand ->> CommandExecutor: MyParameter
CommandExecutor ->> User: result
@enduml
```
This sequence diagram illustrates the interaction between the user, the command executor, and the `MyCommand` class. The `CommandParameterAttribute` is used to specify that the `MyParameter` property represents a command parameter.