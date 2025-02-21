**Readme File**

**Summary**
The OoBDev.AI.Abstractions library contains abstract definitions for ASP.Net Core Extensions. This library provides a set of classes and attributes that can be used to extend the functionality of ASP.Net Core applications. Specifically, it provides two main classes: `ApplicationRightAttribute` and `ApplicationRightRequirementFilter`. These classes are used to manage user rights and permissions in ASP.Net Core applications.

**Technical Summary**

The OoBDev.AI.Abstractions library uses a combination of design patterns and architectural patterns to implement its functionality. Specifically:

* The `ApplicationRightAttribute` class uses the **Interface Segregation Principle** (ISP) design pattern to define a contract for the required rights, allowing multiple clients to implement different sets of rights.
* The `ApplicationRightRequirementFilter` class uses the **Observer pattern** to notify the filter of changes to the user's rights, enabling the filter to react accordingly.
* The library uses a **Layered Architecture** pattern, with the abstractions namespace serving as a layer on top of the ASP.Net Core framework.

**Component Diagram**
```plantuml
@startuml
!include https://raw.githubusercontent.com/plantuml-stdlib/C4-PlantUML/master/C4_Container.puml

Container(b) "OoBDev.AI.Abstractions" {
  Entity(app) "ApplicationRightAttribute" {
    attr=+requiredRights
  }
  Entity(app) "ApplicationRightRequirementFilter" {
    attr=+OnAuthorization
  }
}

Container(f) "ASP.Net Core" {
  Entity(core) "Microsoft.AspNetCore.Mvc" {
    attr=+Authorization
  }
}

Container(u) "User" {
  Entity(user) "Authenticated User" {
    attr=+Rights
  }
}

app ->> User : Requests Rights
User ->> ApplicationRightRequirementFilter : Sends Rights
ApplicationRightRequirementFilter ->> core : Checks Rights
core ->> app : Returns Filter Output
@enduml
```
This diagram shows the relationships between the components in the OoBDev.AI.Abstractions library and the ASP.Net Core framework. The `ApplicationRightAttribute` and `ApplicationRightRequirementFilter` classes are part of the abstractions namespace, which communicates with the ASP.Net Core MVC framework and interacts with the authenticated user to manage rights and permissions.