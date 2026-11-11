# OoBDev.DacFx.Abstractions

Contracts for building a SQL Server DacPac from a SQLCLR assembly, validating one, and merging several DacPacs into one package.

## Contracts

**Table 1 — Contracts (namespace `OoBDev.DacFx`)**

| Type | Role |
|------|------|
| `IDacPacBuilder` | Builds a DacPac from a SQLCLR assembly: `BuildDacPac(assemblyFile, pdbFile, dacpacFile, projectName, projectVersion)`, plus the pieces it is made of (`BuildModel`, `BuildDacMetadata`, `BuildOrigin`, `BuildContentType`, and the aggregate, function, method, user-defined type and file element builders) and the SHA-256 and SHA-512 helpers. |
| `IDacPacValidator` | `ValidateDacPac(dacpacFile)` checks that a package loads. |
| `IDacpacMergeCompiler` | `CreatePackage(definition)` writes one DacPac from an `IDacPacMergeDefinition`. |
| `IDacPacMergeDefinition` | What to merge: `SourceFiles`, `TargetPath`, `ServerVersion`, `ModelOptions`, `TargetBuildVersion` and `TargetPackageMetadata`. |
| `IDacPacMergeTemplate` | The merge request as written by a user: `SourcePath` and `SourcePatterns` to find the sources, `TargetPath`, `Name`, `Description`, `Version`, `BuildVersion`, `ServerVersion`, and `ModelOptions` with the `ModelOptionSource` that says where they come from. |
| `IDacPacMergeTemplateFactory` | `Create()` reads a template (for example from a file or the command line). |
| `IDacPacMergeDefinitionFactory` | `Create(template)` expands a template into a definition (resolves the patterns to files). |
| `IDacPacCompilerConfig` | The same settings as properties of a configuration object (`TemplatePath`, `SourcePath`, `SourcePatterns`, `TargetPath`, `Name`, `Description`, `Version`, `BuildVersion`, `ModelOptionSource`). |
| `ModelOptionSource` | `Custom`, `First` or `Last`: which model options the merged package uses when several sources are merged. |

## Implementations

The builder and validator are in [OoBDev.DacFx](../OoBDev.DacFx/README.DacFx.md) (`services.TryAddDacPacServices()`). The merge compiler and the template and definition factories are in `OoBDev.Microsoft.SqlServer.DacFx` (`services.AddDacPacCompilerServices()`). The `dacpactools` dotnet tool (`OoBDev.DacPacCompiler.Cli`) builds a DacPac only; the merge is not wired into it yet.
