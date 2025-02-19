**Documentation for OoBDev.AspNetCore.Mvc.SwaggerGen**

**Overview**

The OoBDev.AspNetCore.Mvc.SwaggerGen namespace provides additional functionality for SwaggerGen and SwaggerUI in ASP.NET MVC applications. It includes classes for configuring SwaggerGen options, registering filters, and extending the functionality of SwaggerUI.

**Classes and Interfaces**

### AdditionalSwaggerGenEndpointsOptions.cs

This class extends the `SwaggerGenOptions` class to provide additional features for SwaggerGen, such as presenting permissions, application versions, and XML documentation.

**Sequence Diagram**

```plantuml
@startuml
class SwaggerGenOptions
class AdditionalSwaggerGenEndpointsOptions
class ILogger
class IVersionProvider
 AdditionalSwaggerGenEndpointsOptions --|> SwaggerGenOptions
 SwaggerGenOptions ..> ILogger
 SwaggerGenOptions ..> IVersionProvider
logger ..> log message
@enduml
```

### AdditionalSwaggerUIEndpointsOptions.cs

This class extends the `SwaggerUIOptions` class to provide additional features for SwaggerUI, such as grouping controller/actions by assembly.

**Sequence Diagram**

```plantuml
@startuml
class SwaggerUIOptions
class AdditionalSwaggerUIEndpointsOptions
class IActionDescriptorCollectionProvider
 AdditionalSwaggerUIEndpointsOptions --|> SwaggerUIOptions
 SwaggerUIOptions ..> IActionDescriptorCollectionProvider
provider --|> provider_ActionDescriptors
@enduml
```

### AddMvcFilterOptions.cs

This class provides a way to register additional ASP.NET MVC filters.

**Class Diagram**

```plantuml
@startuml
class MvcOptions
class AddMvcFilterOptions
class TFilter
MvcOptions ..> AddMvcFilterOptions
AddMvcFilterOptions ..> TFilter
@enduml
```

### AddOperationFilterOptions.cs

This class provides a way to register additional operation filters for SwaggerGen.

**Class Diagram**

```plantuml
@startuml
class SwaggerGenOptions
class AddOperationFilterOptions
class T
SwaggerGenOptions ..> AddOperationFilterOptions
AddOperationFilterOptions ..> T
@enduml
```

### AddSchemaFilterOptions.cs

This class provides a way to register additional schema filters for SwaggerGen.

**Class Diagram**

```plantuml
@startuml
class SwaggerGenOptions
class AddSchemaFilterOptions
class T
SwaggerGenOptions ..> AddSchemaFilterOptions
AddSchemaFilterOptions ..> T
@enduml
```

**Components**

* **SwaggerGen**: The SwaggerGen component is responsible for generating Swagger documentation for ASP.NET MVC applications.
* **SwaggerUI**: The SwaggerUI component provides a user interface for interacting with Swagger documentation.
* ** ILogger**: The ILogger interface provides a way to log messages from the SwaggerGen and SwaggerUI components.
* **IVersionProvider**: The IVersionProvider interface provides a way to retrieve application versions.

**System Design**

The OoBDev.AspNetCore.Mvc.SwaggerGen namespace is designed to be extensible and flexible. It provides a way to register additional filters and configure SwaggerGen and SwaggerUI options to meet specific requirements.

The classes in this namespace are designed to work together to provide a comprehensive solution for generating Swagger documentation and providing a user interface for interacting with that documentation.