# Architecture § 1 — The Shape in One Page

[↑ Architecture](./README.md) · [← Index](./README.md) · [2. The Five Source Layers →](./02-five-source-layers.md)

C4 style, drawn with plain PlantUML (rectangles + stereotypes, no `!include` of the C4 library).

```plantuml
@startuml
skinparam shadowing false
skinparam defaultTextAlignment center
skinparam rectangle {
  BackgroundColor<<person>> #08427B
  FontColor<<person>> white
  BackgroundColor<<system>> #1168BD
  FontColor<<system>> white
  BackgroundColor<<container>> #438DD5
  FontColor<<container>> white
  BackgroundColor<<external>> #999999
  FontColor<<external>> white
  BorderColor #3C7FC0
}

rectangle "Product developer
[Person]
Builds an application on the framework" as Dev <<person>>
rectangle "OoBDev (dotex) framework
[Software System]
Reusable .NET library suite: composition, providers, messaging, caching, identity, search" as Sys <<system>>
rectangle "Third-party services
[External Systems]
Redis, RabbitMQ, MongoDB, SQL Server, Azure, AWS, Keycloak, Ollama, Qdrant" as Ext <<external>>
rectangle "NuGet feed / CI
[External System]
GitHub Actions, GitVersion, package registry" as Ci <<external>>

Dev --> Sys : references packages,
calls TryAllCommonExtensions
Sys --> Ext : adapters call SDKs
Sys --> Ci : built, versioned and
packaged by
@enduml
```

*Figure 1 — System context (C4 level 1)*

```plantuml
@startuml
skinparam shadowing false
skinparam defaultTextAlignment center
skinparam rectangle {
  BackgroundColor<<person>> #08427B
  FontColor<<person>> white
  BackgroundColor<<system>> #1168BD
  FontColor<<system>> white
  BackgroundColor<<container>> #438DD5
  FontColor<<container>> white
  BackgroundColor<<external>> #999999
  FontColor<<external>> white
  BorderColor #3C7FC0
}

rectangle "Application
[Container: ASP.NET Core / Worker / CLI]
Composition root: Program.cs" as App <<container>>
rectangle "Common
[Container: roll-up projects]
One Try{Layer}Extensions entry point and Builder record per layer" as Common <<container>>
rectangle "Framework
[Container: class libraries]
Domain-neutral capabilities: System, Caching, MessageQueueing, Identity, Search, Documents" as FW <<container>>
rectangle "Extensions
[Container: class libraries]
Optional / niche: vectors + SQL CLR, Html, Markdown, Yaml" as Ext <<container>>
rectangle "Abstractions
[Container: *.Abstractions libraries]
Interfaces, attributes, records" as Abs <<container>>
rectangle "ExternalServices adapters
[Container: class libraries]
One adapter per third-party product" as Ad <<container>>
rectangle "Third-party services
[External Systems]" as X <<external>>
rectangle "Tools and Tests
[Container: CLIs, MSTest projects]" as TT <<container>>

App --> Common : references
Common --> FW : MSBuild globs
Common --> Ext : MSBuild globs
Common --> Ad : MSBuild globs
FW --> Abs : implements
Ext --> FW : uses
Ad --> Abs : implements (never the Framework impl)
Ad --> X : SDK / protocol
TT --> Common : uses
@enduml
```

*Figure 2 — Container view of the solution layers (C4 level 2)*


**Scale (measured):** 123 `.csproj`, ≈1,060 `.cs` files, 42 test projects, 120 projects on `net10.0`
(2 × `netstandard2.1` and 1 × `net48` exist only for the SQL CLR / DacFx vector work).

---

[↑ Architecture](./README.md) · [← Index](./README.md) · [2. The Five Source Layers →](./02-five-source-layers.md)
