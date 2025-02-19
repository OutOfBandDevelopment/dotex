Here is the documentation and class diagram for the provided source code files:

**TikaDocToHtmlConversionHandlerTests.cs**

* This class is a test class for the TikaDocToHtmlConversionHandler handler.
* It inherits from the `TikaToHtmlConversionHandlerTestsBase<T>` class and passes `TikaDocToHtmlConversionHandler` as the type parameter.
* It has two test methods:
	+ `ConvertAsyncTest`: Tests the conversion of a MS Word document to HTML.
	+ `ConvertAsyncTestHarness`: Tests the conversion of a file to HTML, given its source and target content types.

**Class Diagram for TikaDocToHtmlConversionHandlerTests.cs**
```
@startuml
class TikaDocToHtmlConversionHandlerTests {
  - TestContext: TestContext
  - TikaDocToHtmlConversionHandler: TikaDocToHtmlConversionHandler

  TikaDocToHtmlConversionHandlerTests(BaseEntity) {}
  ConvertAsyncTest(string resourceName, string sourceType, string targetType, string ext)
  ConvertAsyncTestHarness(string sourceContentType, string destinationContentType)
}
@enduml
```

**TikaDocToHtmlConversionHandlerTestsBase.cs**

* This class is an abstract base class for testing Tika conversion handlers.
* It provides two abstract methods:
	+ `ConvertAsyncTestHarness`: Tests the conversion of a file to a new format, given its source and target content types.
	+ `ConvertAsyncTestHarness`: Tests the conversion of a file to HTML, given its resource name, source type, target type, and file extension.
* It also provides an implementation for the `ConvertAsyncTestHarness` method, which sets up the test logger and creates a mock Apache Tika client.

**Class Diagram for TikaToHtmlConversionHandlerTestsBase.cs**
```
@startuml
abstract class TikaToHtmlConversionHandlerTestsBase<T> {
  + ConvertAsyncTestHarness(string resourceName, string sourceType, string targetType, string ext, TestContext testContext)
  + ConvertAsyncTestHarness(string sourceContentType, string destinationContentType)
}
@enduml
```

**TikaEpubToHtmlConversionHandlerTests.cs**

* This class is a test class for the TikaEpubToHtmlConversionHandler handler.
* It inherits from the `TikaToHtmlConversionHandlerTestsBase<T>` class and passes `TikaEpubToHtmlConversionHandler` as the type parameter.
* It has two test methods:
	+ `ConvertAsyncTest`: Tests the conversion of an EPUB file to HTML.
	+ `ConvertAsyncTestHarness`: Tests the conversion of an EPUB file to HTML, given its source and target content types.

**TikaOdtToHtmlConversionHandlerTests.cs**

* This class is a test class for the TikaOdtToHtmlConversionHandler handler.
* It inherits from the `TikaToHtmlConversionHandlerTestsBase<T>` class and passes `TikaOdtToHtmlConversionHandler` as the type parameter.
* It has two test methods:
	+ `ConvertAsyncTest`: Tests the conversion of an ODT file to HTML.
	+ `ConvertAsyncTestHarness`: Tests the conversion of an ODT file to HTML, given its source and target content types.

**TikaPdfToHtmlConversionHandlerTests.cs**

* This class is a test class for the TikaPdfToHtmlConversionHandler handler.
* It inherits from the `TikaToHtmlConversionHandlerTestsBase<T>` class and passes `TikaPdfToHtmlConversionHandler` as the type parameter.
* It has two test methods:
	+ `ConvertAsyncTest`: Tests the conversion of a PDF file to HTML.
	+ `ConvertAsyncTestHarness`: Tests the conversion of a PDF file to HTML, given its source and target content types.

**TikaRtfToHtmlConversionHandlerTests.cs**

* This class is a test class for the TikaRtfToHtmlConversionHandler handler.
* It inherits from the `TikaToHtmlConversionHandlerTestsBase<T>` class and passes `TikaRtfToHtmlConversionHandler` as the type parameter.
* It has two test methods:
	+ `ConvertAsyncTest`: Tests the conversion of an RTF file to HTML.
	+ `ConvertAsyncTestHarness`: Tests the conversion of an RTF file to HTML, given its source and target content types.

Note that the above class diagrams are simplified and do not show all the methods, properties, and fields of the classes.