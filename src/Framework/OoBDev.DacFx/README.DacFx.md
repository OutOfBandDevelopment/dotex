# OoBDev.DacFx

Builds a SQL Server DacPac from a SQLCLR assembly and checks the result. The contracts are `IDacPacBuilder` and `IDacPacValidator` in [OoBDev.DacFx.Abstractions](../OoBDev.DacFx.Abstractions/README.DacFx.Abstractions.md).

## Registration

```csharp
services.TryAddDacPacServices();
```

## Building a DacPac

```csharp
dacPacBuilder.BuildDacPac(
    assemblyFileFramework: "MyAssembly.dll",
    assemblyPdbFramework: "MyAssembly.pdb",
    dacpacFile: "output.dacpac",
    projectName: "MyProject",
    projectVersion: "1.0.0");
```

`DacPacBuilder` reads the assembly with `MetadataLoadContext` (the SQLCLR assembly is inspected, never loaded and run), finds the SQL aggregates, functions, methods and user-defined types, writes the model XML, and zips it with the assembly and the symbol file into the package. The `OoBDev.DacPacCompiler.Cli` tool and the `GenerateDacPac` build target (`src/SqlClr.Build.targets`) call it.

## Checking a query is one SELECT

`SelectOnlyGuard.Check(sql)` parses the text with ScriptDom and allows exactly one `SELECT`: it refuses several statements, any other statement kind, `SELECT INTO`, variable assignment, and `OPENROWSET`, `OPENQUERY` and `OPENDATASOURCE`. It is a first line of defence for a query runner; the real guard is a login that can only read.

## Validating

`BuildDacPac` runs the validator on its own output. `DacPacValidator.ValidateDacPac(file)` loads the package with Microsoft DacFx (`DacPackage.Load`, then a `TSqlModel`), logs name, version and the counts of assemblies, scalar functions, aggregates and user-defined types, and checks each one. Any failure is logged and rethrown as `InvalidOperationException("Generated DACPAC is invalid: ...")`.

Merging several DacPacs is a separate package: [OoBDev.Microsoft.SqlServer.DacFx](../../ExternalServices/Microsoft/OoBDev.Microsoft.SqlServer.DacFx/README.Microsoft.SqlServer.DacFx.md).
