**README**

**Summary**

This repository contains unit tests for a set of extension methods for working with streams in .NET. The tests verify the functionality of two methods: `CopyOf()` and `SplitStreamAsync()`.

**Technical Summary**

The `StreamExtensionsTests` class uses a combination of design patterns and architectural patterns to implement unit tests for the `CopyOf()` and `SplitStreamAsync()` methods. The tests utilize the Dependency Injection pattern through the use of the `TestContext` class, which is used to inject dependencies into the test methods.

The `CopyOf()` test method demonstrates the use of the Liskov Substitution Principle, where a copy of a stream is created and verified to be separate from the original stream. The `SplitStreamAsync()` test method showcases the use of asynchronous programming and the .NET Task Parallel Library (TPL) through the `ToBlockingEnumerable()` extension method.

**Component Diagram using PlantUML**

```plantuml
@startuml
class StreamExtensions {
  +CopyOf()
  +SplitStreamAsync()
}

class TestContext {
  +TestContext()
}

class MemoryStream {
  +MemoryStream(byte[])
  +CopyOf()
  +SplitStreamAsync()
}

class Test {
  +CopyOfTest()
  +SplitStreamAsyncTest()
}

StreamExtensions --+. Test
TestContext --*. Test
MemoryStream --*. StreamExtensions
Test --*. MemoryStream

@enduml
```

This component diagram shows the relationships between the `StreamExtensions` class, the `TestContext` class, the `MemoryStream` class, and the `Test` class. The `StreamExtensions` class has dependencies on the `TestContext` class and the `MemoryStream` class, and is used by the `Test` class to perform unit tests.