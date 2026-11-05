# OoBDev.Documents.ExternalTools

Document conversion by running registered external tools (Ghostscript, GhostPCL and similar) as `IDocumentConversionHandler`s.

- `ExternalToolOptions`: name, executable path, argument template (`{input}`, `{output}`, `{dpi}`), accepted formats, output extension, dpi.
- `ExternalToolConversionHandler`: writes the source stream to a temporary folder (`ITempFileFactory`), runs the tool without a shell (each template token becomes one `ArgumentList` entry, so paths with spaces stay one argument), and copies the output file to the destination stream. A tool that cannot start, exits non-zero or writes nothing raises `InvalidOperationException` with the tool's stderr.
- `ContentTypeNames`: maps short names and extensions (`ps`, `pcl`, `hpgl`, `png`, `image`...) to content types.

```csharp
services.AddExternalToolConversions(options.Tools);
```

References `OoBDev.Documents.Abstractions` and `OoBDev.System.Abstractions`. Moved from `OoBDev.DevTerm.Core` ([migration plan](../../../docs/design/StreamTools/migration-plan.md)).
