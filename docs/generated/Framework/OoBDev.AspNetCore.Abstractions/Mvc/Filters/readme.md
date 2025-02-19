Here is the README file with a summary, technical summary, and component diagram:

**README**

This repository contains source files for a set of MVC filters that provide a simple way to implement role-based access control (RBAC) in an ASP.NET Core application.

**Summary**

The `ApplicationRightAttribute` and `ApplicationRightRequirementFilter` classes provide a mechanism to ensure that a user has the required application rights to access a specific endpoint. The `ApplicationRightAttribute` is used to declare the required application rights for an endpoint, while the `ApplicationRightRequirementFilter` is responsible for checking if the authenticated user has at least one of the required rights.

**Technical Summary**

From a technical standpoint, this implementation uses the Factory pattern, specifically the creational sub-pattern Type Factory. The `ApplicationRightAttribute` creates an instance of the `ApplicationRightRequirementFilter` class, passing in the required rights as an argument to the constructor.

The `ApplicationRightRequirementFilter` class implements the IAuthorizationFilter interface, which provides a way to customize the authorization process in ASP.NET Core MVC. It uses the LINQ `Any` method and `Contains` method to check if the user's application rights match the required rights for the endpoint.

```
```plantuml
@startuml
!include https://raw.githubusercontent.com/plantuml-remote/plantuml-remote/master/howto/plantuml-remote.iuml

class ApplicationRightAttribute {
  -rights: string[]
  +getApplicationRightRequirementFilter(rights: string[]) : ApplicationRightRequirementFilter
  +Declare required rights for endpoint
}

class ApplicationRightRequirementFilter {
  -rights: IReadOnlyList<string>
  +ApplicationRightRequirementFilter(rights: string[]) : void
  +Check if authenticated user has at least one of the required rights
  +Ensure that user matches at least one requested right
}

ApplicationRightAttribute --|> ApplicationRightRequirementFilter
@enduml
```