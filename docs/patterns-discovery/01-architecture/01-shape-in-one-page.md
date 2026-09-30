# Architecture § 1 — The Shape in One Page

<!-- nav -->
[↑ 01 — Architecture: How the Solution Is Put Together](./README.md) · [← Index](./README.md) · [Architecture § 2 — The Five Source Layers →](./02-five-source-layers.md)
<!-- nav -->

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

rectangle "Product developer\n[Person]\nBuilds an application on the framework" as Dev <<person>>
rectangle "OoBDev (dotex) framework\n[Software System]\nReusable .NET library suite: composition, providers, messaging, caching, identity, search" as Sys <<system>>
rectangle "Third-party services\n[External Systems]\nRedis, RabbitMQ, MongoDB, SQL Server, Azure, AWS, Keycloak, Ollama, Qdrant" as Ext <<external>>
rectangle "NuGet feed / CI\n[External System]\nGitHub Actions, GitVersion, package registry" as Ci <<external>>

Dev --> Sys : references packages and calls TryAllCommonExtensions
Sys --> Ext : adapters call SDKs
Sys --> Ci : built, versioned and packaged by
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

rectangle "Application\n[Container: ASP.NET Core / Worker / CLI]\nComposition root: Program.cs" as App <<container>>
rectangle "Common\n[Container: roll-up projects]\nOne Try{Layer}Extensions entry point and Builder record per layer" as Common <<container>>
rectangle "Framework\n[Container: class libraries]\nDomain-neutral capabilities: System, Caching, MessageQueueing, Identity, Search, Documents" as FW <<container>>
rectangle "Extensions\n[Container: class libraries]\nOptional / niche: vectors + SQL CLR, Html, Markdown, Yaml" as Ext <<container>>
rectangle "Abstractions\n[Container: *.Abstractions libraries]\nInterfaces, attributes, records" as Abs <<container>>
rectangle "ExternalServices adapters\n[Container: class libraries]\nOne adapter per third-party product" as Ad <<container>>
rectangle "Third-party services\n[External Systems]" as X <<external>>
rectangle "Tools and Tests\n[Container: CLIs, MSTest projects]" as TT <<container>>

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

<!-- nav -->
[↑ 01 — Architecture: How the Solution Is Put Together](./README.md) · [← Index](./README.md) · [Architecture § 2 — The Five Source Layers →](./02-five-source-layers.md)
<!-- nav -->
