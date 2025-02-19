**README**

This repository contains the tests for the OoBDev.AspNetCore.JwtAuthentication, OoBDev.AspNetCore.Mvc, and OoBDev.TestUtilities projects. The tests are written using Microsoft's MSTest framework and are designed to verify the functionality of each project.

**Technical Summary**

The test project is built using the .NET 8.0 framework and utilizes the following design patterns and architectural patterns:

* **MVC Pattern**: The OoBDev.AspNetCore.Mvc project follows the Model-View-Controller pattern, where the controller handles incoming requests, interacts with the model, and returns a response to the view.
* **Repository Pattern**: The OoBDev.AspNetCore.JwtAuthentication project uses the Repository pattern to abstract the data access layer and provide a layer of abstraction between the business logic and the data storage.

**Component Diagram**

Here is a high-level component diagram of the system using PlantUML:
```plantuml
@startuml
class OoBDev.AspNetCore.Mvc(MVC)
class OoBDev.AspNetCore.JwtAuthentication(Authentication)
class OoBDev.TestUtilities(TestUtilities)

OoBDev.AspNetCore.Mvc --* OoBDev.AspNetCore.JwtAuthentication
OoBDev.AspNetCore.Mvc --* OoBDev.TestUtilities

note "Test Driven Development"
@enduml
```