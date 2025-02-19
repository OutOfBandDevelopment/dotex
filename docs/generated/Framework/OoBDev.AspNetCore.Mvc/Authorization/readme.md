**README File**
================

**Summary**
---------

The UserAuthorizationHandler and UserAuthorizationRequirement classes provide functionality for handling user authorization in a web application. The UserAuthorizationHandler class is responsible for checking if a user meets the specified authorization requirements, while the UserAuthorizationRequirement class represents an authorization requirement for user-specific authorization.

**Technical Summary**
-------------------

The UserAuthorizationHandler class uses the AuthorizationHandler abstract class from Microsoft.AspNetCore.Authorization, which is a part of the ASP.NET Core authorization system. The class implements the HandleRequirementAsync method, which checks if the user meets the specified requirements, such as having a valid username and application user ID.

The UserAuthorizationRequirement class derives from IAuthorizationRequirement and has a single property, RequireApplicationUserId, which specifies whether the application user ID is required for authorization.

**Component Diagram**
--------------------

```plantuml
@startuml
class UserAuthorizationHandler {
  - logger: ILogger
  - requirement: UserAuthorizationRequirement
}

class UserAuthorizationRequirement {
  - requireApplicationUserId: bool
}

UserAuthorizationHandler -> UserAuthorizationRequirement: requirement
ILogger -> UserAuthorizationHandler: logger

[Color=black,border=black]
  authContext: AuthorizationHandlerContext
[/Color]
  UserAuthorizationHandler -> authContext: HandleRequirementAsync
  authContext -> UserAuthorizationRequirement: requirement
[/arrow]
[/plantuml]
```

In this diagram, the UserAuthorizationHandler class is responsible for handling user authorization requirements and uses an instance of the UserAuthorizationRequirement class to determine whether the user is authorized. The logger is injected into the handler to provide logging capabilities. The authContext represents the authorization context and is used to handle the user authorization requirement.