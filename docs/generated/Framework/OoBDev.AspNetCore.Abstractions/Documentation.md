Here is the documentation for the source code in Markdown format:

**Introduction**
===============

This documentation provides an overview of the OoBDev.AI.Abstractions library, which contains abstract definitions for ASP.Net Core Extensions.


**Class Diagram**
```plantuml
@startuml
class ApplicationRightAttribute {
  - rights: List<Right>
}

class ApplicationRightRequirementFilter {
  - onAuthorization(context: AuthorizationFilterContext)
  - context: AuthorizationFilterContext
}

@enduml
```


**Component Model**
```plantuml
@startuml
component "OoBDev.AI.Abstractions" {
  Boundary ApplicationRightAttribute
  Boundary ApplicationRightRequirementFilter
}

@enduml
```

**Sequence Diagram**
```plantuml
@startuml
actor "User"
participant "ApplicationRightRequirementFilter"
participant "AuthorizationFilterContext"

User ->> ApplicationRightRequirementFilter: OnAuthorization
ApplicationRightRequirementFilter ->> AuthorizationFilterContext: GetRequestedRights
AuthorizationFilterContext ->> ApplicationRightRequirementFilter: HasRequestedRights
ApplicationRightRequirementFilter ->> User: isAuthorized

@enduml
```
