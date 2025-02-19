**README**

The `OoBDev.Extensions.Configuration` namespace provides a set of utilities for working with configuration data. This package includes two main components: `CommandLine` and `ConfigurationBuilderExtensions`.

**Summary**

The `CommandLine` class provides a builder pattern for command-line arguments. It allows you to define configurable parameters and add them to a dictionary of key-value pairs. The `ConfigurationBuilderExtensions` class provides extension methods for adding in-memory collections to an `IConfigurationBuilder`. This allows you to easily populate a configuration builder with initial data.

**Technical Summary**

The `CommandLine` class uses the builder pattern to construct a dictionary of command-line arguments. It iterates over the properties of a given type, looking for attributes that define the command-line parameters. The `BuildParameters` method groups the parameters by key and returns a dictionary of key-value pairs.

The `ConfigurationBuilderExtensions` class uses the `AddInMemoryCollection` method to add an in-memory collection to an `IConfigurationBuilder`. This method groups the initial data by key and returns a dictionary of key-value pairs.

**Component Diagram**

Here is a component diagram using PlantUML:
```
```plantuml
@startuml
class ICommandLine {
  - CommandLine
}

class IConfigurationBuilder {
  - ConfigurationBuilderExtensions
}

class ConfigurationBuilderExtensions {
  - AddInMemoryCollection
}

CommandLine -->> ICommandLine
ConfigurationBuilder -->> IConfigurationBuilder
ConfigurationBuilder --|> ConfigurationBuilderExtensions
Classification: UML
```plantuml
```
This diagram shows the relationships between the `CommandLine` class, the `IConfigurationBuilder` interface, and the `ConfigurationBuilderExtensions` class. The `CommandLine` class instantiates the `ICommandLine` interface, which is used to construct a dictionary of command-line arguments. The `ConfigurationBuilderExtensions` class provides extension methods for adding in-memory collections to an `IConfigurationBuilder`.