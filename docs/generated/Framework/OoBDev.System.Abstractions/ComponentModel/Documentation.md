Here is the documentation for the source code in Markdown format:

## OoBDev.System.ComponentModel

### Overview

The OoBDev.System.ComponentModel namespace provides a set of attributes and interfaces to help with serialization and state machine management. The namespace consists of four main classes: `EndStateAttribute`, `EnumValueAttribute`, `ExcludeFromUniqueAttribute`, and `IVersionProvider`.

### Classes

#### `EndStateAttribute`

This attribute is used to tag valid end states for enum-based state machines.
```plantuml
@startuml
class EndStateAttribute extends Attribute
attribute "AttributeTargets" : AttributeTargets(Enum | Field)
@enduml
```