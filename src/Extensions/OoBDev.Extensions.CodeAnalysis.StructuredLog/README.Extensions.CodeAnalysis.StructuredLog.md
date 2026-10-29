# OoBDev.Extensions.CodeAnalysis.StructuredLog

XPath navigation over MSBuild binary logs (`.binlog`), migrated from BinaryDataDecoders.CodeAnalysis.StructuredLog. Elements are the log's node types (`Build`, `Project`, `Target`, `Task`, `Message`, `Warning`, `Error`, `Property`, ...) and their facts are attributes (`Name`, `Code`, `LineNumber`, `Duration`, ...). Numbers and dates are formatted with the invariant culture.

| Type | Purpose |
|------|---------|
| `StructuredLogNavigator` | `[FileExtension(".binlog")]`; `ToNavigable(path or stream)`. |
| `StructuredLogNavigatorFactory` | `ToNavigable(Build)` and `AsNode(Build)` for a build you already loaded. |

```csharp
// dotnet build -bl:build.binlog
var navigable = new StructuredLogNavigator().ToNavigable("build.binlog")!;
var warnings = navigable.CreateNavigator()!.Select("//Warning/@Code");
```

Tests: `OoBDev.Extensions.CodeAnalysis.StructuredLog.Tests` (Simulate: each test runs `dotnet msbuild` on a one-target project to write a real log; a hand-built `Build` object cannot be used because several node properties need the loader's string table).
