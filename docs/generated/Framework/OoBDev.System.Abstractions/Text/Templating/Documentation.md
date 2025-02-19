**Introduction**
===============

This documentation describes the source code of the OoBDev System Text Templating library, which provides a template engine for generating content based on templates. The library defines several interfaces, classes, and methods for managing file types, template contexts, and template engines.

**Overview of Interfaces**
-------------------------

### IFileType

The `IFileType` interface defines a set of properties that describe a file type, including its extension, content type, and whether it is a template type.

### IFileTypeProvider

The `IFileTypeProvider` interface provides a collection of `IFileType` objects.

### ITemplateContext

The `ITemplateContext` interface represents the context for a template, including information about the template and target content types, source, and priority.

### ITemplateEngine

The `ITemplateEngine` interface defines methods for managing templates, including getting and applying templates, and writing results to a stream or returning as a string.

**Class Diagram**
----------------

```plantuml
@startuml
class IFileType {
  - extension: string
  - contentType: string
  - isTemplateType: bool
}

class IFileTypeProvider {
  + Types: IReadOnlyCollection<IFileType>
}

class ITemplateContext {
  - templateName: string
  - templateContentType: string
  - templateFileExtension: string
  - templateSource: ITemplateSource
  - templateReference: string
  - openTemplate: Func<ITemplateContext, Stream>
  - targetContentType: string
  - targetFileExtension: string
  - priority: int
}

class ITemplateEngine {
  + Get(templateName: string): ITemplateContext?
  + GetAll(templateName: string): IEnumerable<ITemplateContext>
  + ApplyAsync(templateName: string, data: object, target: Stream): Task<ITemplateContext?>
  + ApplyAsync(context: ITemplateContext, data: object, target: Stream): Task<bool>
  + ApplyAsync(templateName: string, data: object): Task<string?>
  + ApplyAsync(context: ITemplateContext, data: object): Task<string?>
}
@enduml
```

**Sequence Diagram - Applying a Template**
----------------------------------------

```plantuml
@startuml
title Applying a Template

 participant "Template Engine" as TE
 participant "Data" as D
 participant "Stream" as S

 note left
 Apply template with name "{templateName}" using data {data}
 end note

 TE ->> D: Get data
 D ->> TE: Data
 TE ->> S: Get stream
 TE ->> TE: Open template
 S ->> TE: Write result

 TE ->> D: Apply template to data
 D ->> TE: Data and template context
 TE ->> S: Write result
 note right
 Async operation
 end note

 S ->> TE: Stream result
 TE ->> S: Continue writing
 TE ->> TE: Apply template asynchronous
 TE ->> S: Result to stream
 TE ->> D: async boolean
 D ->> TE: boolean
 TE ->> S: Result to stream
 note right
 Async operation
 end note

 TE ->> D: Get name and data
 D ->> TE: Name and data
 TE ->> TE: Get template context
 TE ->> S: Get stream
 TE ->> TE: Open template
 S ->> TE: Write result

 TE ->> D: Get template context and data
 D ->> TE: Template context and data
 TE ->> S: Write result
 TE ->> D: async boolean
 D ->> TE: boolean
 TE ->> S: Result to stream
 note right
 Async operation
 end note
@enduml
```

**Component Model**
-----------------

The OoBDev System Text Templating library consists of the following components:

* `IFileType`: represents a file type
* `IFileTypeProvider`: provides a collection of `IFileType` objects
* `ITemplateContext`: represents a template context
* `ITemplateEngine`: manages templates

These components interact with each other through interfaces and methods to provide a template engine for generating content based on templates.