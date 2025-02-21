**README**

The OoBDev.AspNetCore.Mvc.SwaggerGen project provides a set of extensions for ASP.NET Core MVC to enhance the Swagger/OpenAPI documentation for .NET Core applications.

**Overview**

The project consists of five main classes:

* `AdditionalSwaggerGenEndpointsOptions`: Extends the SwaggerGen options to configure aspects such as presenting permissions, application versions, and XML documentation.
* `AdditionalSwaggerUIEndpointsOptions`: Extends the SwaggerUI options to enable grouping of controller/actions by assembly.
* `AddMvcFilterOptions<TFilter>`: Registers additional ASP.NET Core MVC filters.
* `AddOperationFilterOptions<T>`: Registers additional IOperationFilters for SwaggerGen.
* `AddSchemaFilterOptions<T>`: Registers additional ISchemaFilters for SwaggerGen.

**Technical Summary**

The project uses various design patterns and architectural patterns:

*   **Factory Pattern**: `AddMvcFilterOptions<TFilter>`, `AddOperationFilterOptions<T>`, and `AddSchemaFilterOptions<T>` classes act as factories, creating and configuring the specified filters and options.
*   **Strategy Pattern**: `AdditionalSwaggerGenEndpointsOptions` class uses the strategy pattern to configure different aspects of SwaggerGen options.
*   **Observer Pattern**: SwaggerGen and SwaggerUI options are observed and configured using the observer pattern.

**Component Diagram**

Here is a component diagram illustrating the relationships between the classes and options:
```plantuml
@startuml
class IConfigureOptions[options] {
    - SwaggerGenOptions
    - SwaggerUIOptions
}

class SwaggerGenOptions {
    - OpenApiInfo
    - OperationFilter
    - SchemaFilter
}

class SwaggerUIOptions {
    - SwaggerEndpoint
    - ShowExtensions
}

class AdditionalSwaggerGenEndpointsOptions {
    - IActionDescriptorCollectionProvider
    - ILogger
    - IEnumerable<IVersionProvider>
    - resolves SwaggerGenOptions
}

class AdditionalSwaggerUIEndpointsOptions {
    - IActionDescriptorCollectionProvider
    - resolves SwaggerUIOptions
}

class AddMvcFilterOptions<TFilter> {
    - TFilter
    - resolves MvcOptions
}

class AddOperationFilterOptions<T> {
    - T
    - resolves SwaggerGenOptions
}

class AddSchemaFilterOptions<T> {
    - T
    - resolves SwaggerGenOptions
}

IConfigureOptions[options] --|> SwaggerGenOptions
IConfigureOptions[options] --|> SwaggerUIOptions
AdditionalSwaggerGenEndpointsOptions --|> SwaggerGenOptions
AdditionalSwaggerUIEndpointsOptions --|> SwaggerUIOptions
AddMvcFilterOptions<TFilter> --|> MvcOptions
AddOperationFilterOptions<T> --|> SwaggerGenOptions
AddSchemaFilterOptions<T> --|> SwaggerGenOptions
@enduml
```
Note: The diagram only shows the main relationships between the classes and options, and may not include all the dependencies and relationships.