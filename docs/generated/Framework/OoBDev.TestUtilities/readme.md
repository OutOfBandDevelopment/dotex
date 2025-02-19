**README File for OoBDev.TestUtilities**

**Summary**
OoBDev.TestUtilities is a collection of extension methods and helper classes for improving unit tests within your projects. It provides features such as easy serialization of test results, deserialization of test data from embedded resources, and logging utilities.

**Technical Summary**

The project uses several design patterns and architectural patterns, including:

* **Single Responsibility Principle**: Each class and method has a single responsibility and is designed to perform a specific task.
* **Dependency Injection**: The project uses dependency injection to inject instances of `ILoggerFactory` and `IServiceProvider` into constructors.
* **Null Object Pattern**: The `TestLogger` class uses the null object pattern to provide a default implementation of `ILoggerFactory` when no instance is available.
* **Repository Pattern**: The `TestContext` class uses a repository pattern to store and retrieve test results.

**Component Diagram**

[```plantuml
@startuml
class TestContext {
  - testResultsDirectory: string
  - testRunResultsDirectory: string
  - testResults: Dictionary<string, string>
  - testResultsFileNames: List<string>
  + constructor()
  + addResult(object, string, ...optionalParams...)
  + addResultFile(string, byte[])
  + getTestData(Type, string, IServiceProvider)
  + getQualifierdTestName()
  + getTestRunResultFiles()
  + resolveTestType()
}

class TestLogger {
  - factory: ILoggerFactory
  + getFactory()
  + createLogger<T>()
}

class LoggerFactory {
  + createLogger<T>()
}

class IServiceProvider {
  + getServices()
}

@TestContext --> TestLogger
@TestContext --> LoggerFactory
@TestLogger --> LoggerFactory

@enduml
```]

**Description**

The component diagram shows the relationships between the main classes in the project: `TestContext`, `TestLogger`, and `LoggerFactory`. The `TestContext` class is the central class that provides functionality for adding test results, deserializing test data, and resolving test types. The `TestLogger` class is responsible for creating logger instances and providing a default implementation of `ILoggerFactory`. The `LoggerFactory` class is an interface that creates logger instances. The `IServiceProvider` class is used to provide services to the `TestContext` class.