**README.md**

**Summary**

The OoBDev.AspNetCore.Mvc project provides extensions for ASP.NET Core, including authentication, authorization, search query support, and more. These extensions can be used to customize and enhance the functionality of ASP.NET Core applications.

**Technical Summary**

The project uses several design patterns and architectural patterns, including:

* **Service-based architecture**: The project uses a service-based architecture, where services are registered with an instance of `IServiceCollection` and can be injected into other services and components.
* **Singletons**: The project uses singletons to store and manage shared instances of certain objects, such as the current culture and search query results.
* **Factories**: The project uses factories to create instances of certain objects, such as HTTP context accessors and search query result filters.

**Component Diagram**

```plantuml
@startuml
!include https://raw.githubusercontent.com/plantuml-stdlib/C4-PlantUML/master/C4_Context.puml

System_Boundary(as "OoBDev.AspNetCore.Mvc") {
  ServiceProvider(as "Service Provider") {
    Package_Control(ASPNETCoreExtensions) {
      Component(HealthCheckOptionsFactory, "HealthCheckOptionsFactory") 
      Component(AuthorizationPolicyBuilder, "AuthorizationPolicyBuilder") 
      Component(SearchQueryMiddleware, "SearchQueryMiddleware") 
    }
  }
  Application(as "Application") {
    Component(Controller, "Controller") 
    Component(IAuthorizationHandler, "UserAuthorizationHandler") 
    Component(ISearchQuery, "SearchQuery") 
    Component(IConfigureOptions, "ConfigureOptions") 
  }
  Infrastructure(as "Infrastructure") {
    Component(HttpContextAccessor, "HttpContextAccessor") 
    Component(IActionContextAccessor, "ActionContextAccessor") 
  }
}
@enduml
```

This component diagram shows the high-level structure of the OoBDev.AspNetCore.Mvc project, including the service provider, application, and infrastructure components. The diagram includes various components, such as the health check options factory, authorization policy builder, search query middleware, and HTTP context accessor.