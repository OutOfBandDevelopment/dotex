# OoBDev.AspNetCore.Tests Documentation

## Project File

```plantuml
@startuml
class Project {
  -targetFramework: Target .NET 8.0
  -implicitUsings: False
  -nullableReferenceTypes: Enable
  -isPackable: False
  -isTestProject: True
}

Project --* PropertyGroup
PropertyGroup --* PackageReference
PackageReference --* Microsoft.NET.Test.Sdk
PackageReference --* MSTest.TestAdapter
PackageReference --* MSTest.TestFramework
PackageReference --* coverlet.collector

Project --* ItemGroup
ItemGroup --* ProjectReference
ProjectReference --* OoBDev.AspNetCore.JwtAuthentication
ProjectReference --* OoBDev.AspNetCore.Mvc
ProjectReference --* OoBDev.TestUtilities

@enduml
```

```plantuml
```