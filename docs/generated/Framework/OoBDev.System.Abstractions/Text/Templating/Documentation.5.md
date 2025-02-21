# OoBDev.System.Text.Templating Documentation

## Overview

The OoBDev.System.Text.Templating namespace provides a template engine that allows for the application of templates based on a specified context. The engine consists of two main components:

* `ITemplateProvider`: An interface that defines the contract for a template provider, which can apply templates based on a specified context.
* `ITemplateSource`: An interface that defines the contract for a template source, which provides a source of templates to be used by the template engine.

### ITemplateProvider

The `ITemplateProvider` interface defines the contract for a template provider. It has the following methods:

#### ITemplateProvider.SupportedContentTypes

Gets the collection of supported content types by the template provider.

#### ITemplateProvider.CanApply

Determines whether the template provider can apply a template based on the provided context.

#### ITemplateProvider.ApplyAsync

Applies the template associated with the specified context, using the provided data, and writes the result to the target stream asynchronously.

```plantuml
@startuml
interface ITemplateProvider {
  *-} SupportedContentTypes: IReadOnlyCollection<string>
  *-} CanApply: bool (ITemplateContext)
  *-} ApplyAsync: Task<bool> (ITemplateContext, object, Stream)
}
@enduml
```

### ITemplateSource

The `ITemplateSource` interface defines the contract for a template source. It has the following method:

#### ITemplateSource.Get

Gets the template contexts associated with the specified template name.

```plantuml
@startuml
interface ITemplateSource {
  *-} Get: IEnumerable<ITemplateContext> (string)
}
@enduml
```

### Component Model

The following component model illustrates the relationship between the `ITemplateProvider` and `ITemplateSource` interfaces:
```plantuml
@startuml
component TemplateEngine << interface >> {
  .. ITemplateProvider
  .. ITemplateSource
}
component TemplateSource << interface >> {
  .. ITemplateSource
}
component TemplateProvider << interface >> {
  .. ITemplateProvider
}
TemplateEngine ..> TemplateSource : uses
TemplateEngine ..> TemplateProvider : uses
@enduml
```

### Sequence Diagram

The following sequence diagram illustrates the flow of a template application using the `ITemplateProvider` and `ITemplateSource` interfaces:
```plantuml
@startuml
autonumber 1
Alice ->> Bob: Get template name
Bob ->> Charlie: Get template contexts
Charlie ->> Bob: Return template contexts
Bob ->> Alice: Select template context
Alice ->> Bob: Get data
Bob ->> Charlie: Apply template
Charlie ->> Bob: Return result
Bob ->> Alice: Write result to stream
@enduml
```

## Implementation

The implementation of the `ITemplateProvider` and `ITemplateSource` interfaces is the responsibility of the template engine developers. The engine should implement these interfaces to provide a functional template application mechanism.