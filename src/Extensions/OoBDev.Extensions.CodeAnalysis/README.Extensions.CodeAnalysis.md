# OoBDev.Extensions.CodeAnalysis

XPath navigation over Roslyn syntax trees (C# and Visual Basic), migrated from BinaryDataDecoders.CodeAnalysis. A syntax tree becomes an `IXPathNavigable` where each element is a syntax node or token kind and facts are attributes, so structure can be queried with XPath.

| Type | Purpose |
|------|---------|
| `CSharpNavigator` | `[FileExtension(".cs")]`; `ToNavigable(path or stream)` parses C# and returns the syntax tree. |
| `CSharpSemanticNavigator` | Same, but compiles the source and adds a `Symbol` child (name, kind, containing type) under bound nodes. |
| `VisualBasicNavigator` | `[FileExtension(".vb")]`; the Visual Basic equivalent of `CSharpNavigator`. |
| `SyntaxTreeNavigatorFactory` | `ToNavigable(SyntaxTree, baseUri)` and `AsNode` for a tree you already hold. |
| `SemanticModelNavigatorFactory` | The same with a `SemanticModel`. |
| `SyntaxNames` | Namespace URI and element-name helpers (`oobdev:CodeAnalysis/csharp`, `.../vb`). |

Attributes: `RawKind`, `Location.Start`, `Location.End`, and `Language` on the root.

```csharp
var navigable = new CSharpNavigator().ToNavigable("Program.cs")!;
var methods = navigable.CreateNavigator()!.Select("//*[local-name()='MethodDeclaration']");
```

DacPac and MSBuild binary logs are separate packages so the SQL and MSBuild dependencies stay optional: [DacFx](../OoBDev.Extensions.CodeAnalysis.DacFx/README.Extensions.CodeAnalysis.DacFx.md) and [StructuredLog](../OoBDev.Extensions.CodeAnalysis.StructuredLog/README.Extensions.CodeAnalysis.StructuredLog.md).

Tests: `OoBDev.Extensions.CodeAnalysis.Tests` (Unit).
