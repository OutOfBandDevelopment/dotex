**README**

This repository contains a set of .NET classes and interfaces that provide a text templating system. The system is designed to allow for the rendering of templates with dynamic data, allowing for the separation of concerns between template authors and application developers.

**Summary**

The system consists of several key components:

* `FileType`: Represents a file type, providing information about the file extension, content type, and whether it is a template type.
* `IFileType`: An interface that allows objects to provide information about a specific file type.
* `IFileTypeProvider`: Provides a collection of file types.
* `ITemplateContext`: Represents the context for a template, including information about the template, target content types, source, and priority.
* `ITemplateEngine`: Represents a template engine for generating content based on templates.

The `ITemplateEngine` interface provides methods for retrieving template contexts, applying data to templates, and writing the result to a target stream.

**Technical Summary**

The system uses the following design patterns and architectural patterns:

* The `FileType` and `IFileType` classes follow the Factory pattern, allowing for the creation of objects that can provide information about different file types.
* The `IFileTypeProvider` interface follows the Provider pattern, allowing for the creation of objects that can provide collections of file types.
* The `ITemplateContext` and `ITemplateEngine` interfaces follow the Bridge pattern, allowing for the decoupling of template contexts and engines.
* The system uses a singleton pattern for the `ITemplateEngine` instance.

**Component Diagram**

```plantuml
@startuml
!include ./uml/Arrow.iuml
class FileType {
  - Extension: string
  - ContentType: string
  - IsTemplateType: bool
}

class IFileType {
  - Extension: string
  - ContentType: string
  - IsTemplateType: bool
}

class IFileTypeProvider {
  - Types: IReadOnlyCollection<IFileType>
}

class ITemplateContext {
  - TemplateName: string
  - TemplateContentType: string
  - TemplateFileExtension: string
  - TemplateSource: ITemplateSource
  - TemplateReference: string
  - OpenTemplate: Func<ITemplateContext, Stream>
  - TargetContentType: string
  - TargetFileExtension: string
  - Priority: int
}

class ITemplateEngine {
  + Get: ITemplateContext?
  + GetAll: IEnumerable<ITemplateContext>
  + ApplyAsync: Task<bool>
  + ApplyAsync: Task<string?>
}

FileType -- IFileType
IFileType -- IFileTypeProvider
ITemplateContext -- ITemplateEngine
@enduml
```

This diagram shows the relationships between the different classes and interfaces. The `FileType` and `IFileType` classes are related by inheritance, while the `IFileTypeProvider` interface is related to the `IFileType` interface. The `ITemplateContext` and `ITemplateEngine` interfaces are related by composition, with the `ITemplateEngine` instance relying on the `ITemplateContext` interface to perform its operations.