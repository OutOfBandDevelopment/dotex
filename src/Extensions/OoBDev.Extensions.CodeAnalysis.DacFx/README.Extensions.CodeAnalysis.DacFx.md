# OoBDev.Extensions.CodeAnalysis.DacFx

XPath navigation over SQL Server data-tier packages (`.dacpac`), migrated from BinaryDataDecoders.CodeAnalysis.DacPac.

| Type | Purpose |
|------|---------|
| `DacPacNavigator` | `[FileExtension(".dacpac")]`; `ToNavigable(path or stream)` loads the package model. A stream is copied to a temporary file first because DacFx loads from a path. |
| `DacPacElementNodeBuilder` | Reflection-based node builder over a `TSqlModel`: one element per object type (`Table`, `Column`, ...), `Name` and `ref-id` attributes, and a `SCRIPT` child with the object's T-SQL. |

```csharp
var navigable = new DacPacNavigator().ToNavigable("Database.dacpac")!;
var tables = navigable.CreateNavigator()!.Select("//Table/@Name");
```

Tests: `OoBDev.Extensions.CodeAnalysis.DacFx.Tests` (Unit; builds a model and a package in memory and in a temp file).
