**Documentation for OoBDev.Markdig.Tests**

**Class Diagram**

The following Plant UML class diagram represents the classes in the OoBDev.Markdig.Tests project:
```
@startuml
class MarkdownToHtmlConversionHandler {
  -serviceProvider: ServiceCollection
  -logger: ILogger
  -markdigServices: MarkdigServices
  +ConvertAsync(stream, sourceType, pdfStream, targetType): Task
}

class TestContext {
  -TestContext: TestContext
  +WriteLine(string): void
  +AddResult(stream, fileName): void
}

class Tests {
  -TestContext: TestContext
  +ConvertAsyncTest(string, string, string): Task
}

MarkdownToHtmlConversionHandler --* Tests
Tests --* TestContext
@enduml
```
**Project Description**

The OoBDev.Markdig.Tests project is a test project for the MarkdownToHtmlConversionHandler class, which converts Markdown to HTML. The project uses Microsoft.NET.Test.Sdk and MSTest.TestAdapter for unit testing. The project references OoBDev.TestUtilities and OoBDev.Markdig projects.

**Class Description**

* **MarkdownToHtmlConversionHandler**: This class is responsible for converting Markdown to HTML. It takes a service provider, logger, and markdig services as dependencies.
* **TestContext**: This class represents the test context. It has methods to write to the console and add results.
* **Tests**: This class is a test class that tests the MarkdownToHtmlConversionHandler class.

**Method Description**

* **ConvertAsync**: This method converts a Markdown stream to an HTML stream using the provided service provider, logger, and markdig services. It takes a source type and target type as parameters.
* **ConvertAsyncTest**: This method tests the ConvertAsync method with different resource names, source types, and target types. It creates a service provider, logger, and markdig services, and then calls the ConvertAsync method. It then verifies that the resulting stream is not empty.

** Notes **

* The project uses the coverlet.collector package to collect code coverage data.
* The project uses the MSTest/TestAdapter package for unit testing.
* The project references OoBDev.TestUtilities and OoBDev.Markdig projects.