# BinaryDataDecoders Code analysis

**Date:** 2026-10-29

The `BinaryDataDecoders.CodeAnalysis*` navigators (875 lines, 360 lines of tests) became three packages so heavy dependencies stay optional:

- `OoBDev.Extensions.CodeAnalysis` ([readme](../../src/Extensions/OoBDev.Extensions.CodeAnalysis/README.Extensions.CodeAnalysis.md)): Roslyn C# and Visual Basic syntax and semantic navigators (12 Unit tests).
- `OoBDev.Extensions.CodeAnalysis.DacFx` ([readme](../../src/Extensions/OoBDev.Extensions.CodeAnalysis.DacFx/README.Extensions.CodeAnalysis.DacFx.md)): `.dacpac` navigator (4 Unit tests).
- `OoBDev.Extensions.CodeAnalysis.StructuredLog` ([readme](../../src/Extensions/OoBDev.Extensions.CodeAnalysis.StructuredLog/README.Extensions.CodeAnalysis.StructuredLog.md)): `.binlog` navigator (3 Simulate tests, 1 Unit).

**Changes from the source:**
- Namespaces moved to `OoBDev.Extensions.CodeAnalysis*`; all use the `ExtensibleElementNode` infrastructure in `OoBDev.System.Xml.XPath`.
- Attribute values are formatted with the invariant culture (analyzers CA1305 and MA0011).
- The structured-log attribute switch uses one helper instead of repeating the projection per node type.
- Tests: the structured-log tests write a real binlog with `dotnet msbuild` (hand-built `Build` objects throw inside `Message.LineNumber` because they lack the loader's string table); the DacFx tests build a `TSqlModel` and a package in memory.
- `Microsoft.CodeAnalysis.VisualBasic` and `MSBuild.StructuredLogger` were added to `Directory.Packages.props`; `System.Security.Cryptography.Pkcs` is pinned in the DacFx project to clear the transitive vulnerability warnings.
- Known source quirk kept as-is: the `_collected` guard in `DacPacElementNodeBuilder` never filters anything; the tests show the walk still terminates on a real model.
