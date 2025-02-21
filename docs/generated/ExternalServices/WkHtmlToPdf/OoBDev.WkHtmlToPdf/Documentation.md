Here is the documentation for each source code file, including a class diagram in PlantUML for the classes in the `OoBDev.WkHtmlToPdf` namespace:

**OoBDev.WkHtmlToPdf.csproj**

This is a .NET Core project file that specifies the project settings, packages, and dependencies.

**HtmlToPdfConversionHandler.cs**

[plantuml]
```
@startuml
class HtmlToPdfConversionHandler {
  - private readonly IConverter _converter
  + ConvertAsync(stream source, string sourceContentType, stream destination, string destinationContentType)
  + SupportedDestination(string contentType)
  + SupportedSource(string contentType)
}

class IConverter { }
class IDocumentConversionHandler { }
class WkHtmlToPdfDocument { }
class WkHtmlToPdf {
  - Objects[*]
  - GlobalSettings[*]
}
@enduml
```

This file defines a class `HtmlToPdfConversionHandler` that implements `IDocumentConversionHandler` and handles the conversion of HTML content to PDF format. The class has several methods:

* `ConvertAsync`: converts HTML content from a source stream to PDF format and writes it to a destination stream.
* `SupportedDestination`: checks if a given content type is supported as a destination for PDF conversion.
* `SupportedSource`: checks if a given content type is supported as a source for HTML conversion.
* The class also has properties for the supported content types for destination PDF files and source HTML files.

**Readme.WkHtmlToPdf.md**

This is a README file for the project that provides a brief summary and some notes about the project.

**ServiceCollectionExtensions.cs**

[plantuml]
```
@startuml
class ServiceCollectionExtensions {
  + TryAddWkHtmlToPdfServices(IServiceCollection services)
}

class IServiceCollection { }
class IConverter { }
class IDocumentConversionHandler { }
class ITools { }
class DocumentType {
  + Name
  + FileHeader
  + FileExtensions
  + ContentTypes
}

@enduml
```

This file defines a class `ServiceCollectionExtensions` that provides extension methods for configuring services related to WkHtmlToPdf. The class has a single method:

* `TryAddWkHtmlToPdfServices`: configures services for WkHtmlToPdf by adding instances of `IDocumentConversionHandler`, `ITools`, and `IConverter`. It also adds an instance of `DocumentType`, which represents a document type with name, file header, file extensions, and content types.