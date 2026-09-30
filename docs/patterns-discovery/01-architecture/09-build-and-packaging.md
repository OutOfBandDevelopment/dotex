# Architecture § 9 — Build & Packaging Architecture

[↑ Architecture](./README.md) · [← 8. Hosting Model](./08-hosting-model.md) · [10. Architectural Principles (Distilled from the Code) →](./10-architectural-principles.md)

* **Two-level MSBuild:** `Directory.Build.props` (metadata, paths, packaging inference, README + license packing, test-project wiring) and `Directory.Build.targets` (custom targets: `DeepClean`, `CleanPaths`, `GetDocumentation`, README enforcement).
* **Self-documenting packages:** every `**\*.md` and `*.plantuml/*.puml` is copied to `docs/code/{Project}` and packed under `\docs`; `**\*.json/html/csv/sql/xml/yml/txt` are embedded as resources and packed under `\examples`.
* **Versioning:** `GitVersion.MsBuild` referenced from each csproj, `mode: ContinuousDeployment`, branch name becomes pre-release label.
* **Test wiring:** `TestAssemblyInfo.cs` (class-level parallelism) is linked into every `*.Tests` project; `.runsettings` supplies coverage (coverlet) and test properties.
* **Multi-runtime islands:** SQL CLR (`net48`/`netstandard2.1`) and DacPac (`MSBuild.Sdk.SqlProj`) projects use their own SDKs and are excluded from Common by name suffix (`.Net481`, `.DB`).

See [Practices and Conventions](../03-practices-and-conventions/README.md) for the checklists.

---

[↑ Architecture](./README.md) · [← 8. Hosting Model](./08-hosting-model.md) · [10. Architectural Principles (Distilled from the Code) →](./10-architectural-principles.md)
