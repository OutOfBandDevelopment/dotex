# OoBDev.Microsoft.SqlServer.DacFx

Merges several DacPacs into one package (`IDacpacMergeCompiler`) and expands a merge template into the list of files to merge. The contracts are in [OoBDev.DacFx.Abstractions](../../Framework/OoBDev.DacFx.Abstractions/README.DacFx.Abstractions.md).

## Registration

```csharp
services.AddDacPacCompilerServices();
```

Registers as singletons (`TryAdd`): `IDacpacMergeCompiler`, `IDacPacCompilerConfig`, `IDacPacMergeTemplateFactory`, `IDacPacMergeDefinitionFactory`, and the `IDacPacMergeTemplate` and `IDacPacMergeDefinition` they produce. The template factory needs an `IObjectConverter` (`OoBDev.System.ComponentModel`) and an `ILogger`; `IDacPacCompilerConfig` is bound from configuration.

## How a merge runs

1. `DacPacMergeTemplateFactory.Create()` reads the template file named by `IDacPacCompilerConfig.TemplatePath` through the object converter (an empty path gives an empty template, a missing file throws `FileNotFoundException`), then lets every non-empty config property override the template: source path and patterns, target path, version, build version, name, description and model option source. YAML extensions are not detected yet; the converter decides the format.
2. `DacPacMergeDefinitionFactory.Create(template)` resolves `SourcePatterns` against `SourcePath` with file globbing (a pattern starting with `!` excludes), picks the model options (`First` and `Last` take them from the first or last matched DacPac, otherwise the template's), and fills the package metadata: name defaults to the target file name, version to `0.0.0`, build version to the version, description to the local time.
3. `DacpacMergeCompiler.CreatePackage(definition)` opens each source DacPac, adds its objects to one `TSqlModel`, collects the pre- and post-deployment scripts of every source, appends a build-version script when `TargetBuildVersion` is set, writes the package (creating the folder) and then adds the combined scripts.

```csharp
compiler.CreatePackage(definitionFactory.Create(template));
```

Object options (`TSqlObjectOptions`) are not carried over from the sources (a TODO in the code). `DacTools` holds the helpers: model options, reading objects and scripts from a DacPac, adding script content and generating the build-version script. The `dacpactools` CLI does not call this merge yet; see its readme.
