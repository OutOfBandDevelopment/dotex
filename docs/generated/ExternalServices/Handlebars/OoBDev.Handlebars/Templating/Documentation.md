Here is the generated documentation for the `HandlebarsTemplateProvider` class:

**Class Name:** `HandlebarsTemplateProvider`

**Namespace:** `OoBDev.Handlebars.Templating`

**Summary:** Provides Handlebars template processing functionality.

**Description:** This class provides a set of methods and properties for processing Handlebars templates.

**Constructor:**
```
public HandlebarsTemplateProvider(IEnumerable<IHelpersRegistry> helpersRegistry,
                                  IEnumerable<IHelperDescriptor<BlockHelperOptions>> blockHelperDescriptors,
                                  IEnumerable<IHelperDescriptor<HelperOptions>> helperDescriptors,
                                  ILogger<HandlebarsTemplateProvider> log)
    : ITemplateProvider
```
* **Parameters:**
	+ `helpersRegistry`: A collection of helpers registries.
	+ `blockHelperDescriptors`: A collection of helpers registries for block helpers.
	+ `helperDescriptors`: A collection of helpers registries for regular helpers.
	+ `log`: The logger instance.

**Properties:**

* **SupportedContentTypes**: Gets the collection of supported content types by the template provider. (These are the MIME types that the template provider can process.)

**Methods:**

* **CanApply(ITemplateContext context)**: Determines whether the template provider can apply template processing to the given context.
* **ApplyAsync(ITemplateContext context, object data, Stream target)**: Asynchronously applies Handlebars template processing to the specified context, data, and target stream.

**Class Diagram (PlantUML):**
```plantuml
@startuml
class HandlebarsTemplateProvider {
  -+ SupportedContentTypes: IReadOnlyCollection<string>
  -+ CanApply(ITemplateContext context): bool
  -+ ApplyAsync(ITemplateContext context, object data, Stream target): Task<bool>
  -+ logger: ILogger<HandlebarsTemplateProvider>

  constructor(helpersRegistry, blockHelperDescriptors, helperDescriptors, log) : ITemplateProvider
  helpersRegistry: IEnumerable<IHelpersRegistry>
  blockHelperDescriptors: IEnumerable<IHelperDescriptor<BlockHelperOptions>>
  helperDescriptors: IEnumerable<IHelperDescriptor<HelperOptions>>

  @enduml
```
This class diagram shows the relationships between the properties, methods, and constructor of the `HandlebarsTemplateProvider` class.

Note that the `IHelpersRegistry`, `IHelperDescriptor<BlockHelperOptions>`, and `IHelperDescriptor<HelperOptions>` interfaces are not shown in this diagram, as they are not defined in the provided source code.