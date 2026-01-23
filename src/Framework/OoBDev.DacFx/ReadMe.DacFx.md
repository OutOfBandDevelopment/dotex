# OoBDev.DacFx

SQL Server DacPac builder and validator implementation.

## Description

This package provides comprehensive DacPac building capabilities, extracting SQL CLR metadata from .NET assemblies and generating valid SQL Server deployment packages. It uses MetadataLoadContext for assembly reflection and creates compliant DacPac ZIP archives.

## Key Features

- .NET assembly to DacPac conversion
- SQL CLR type extraction (UDT, aggregates, functions, methods)
- MetadataLoadContext-based reflection
- DacPac validation using Microsoft DacFx
- XML model generation
- Assembly and PDB embedding
- Type mapping for SQL Server types

## Installation

```xml
<PackageReference Include="OoBDev.DacFx" Version="*" />
```

## Basic Usage

```csharp
using OoBDev.DacFx;
using Microsoft.Extensions.DependencyInjection;

services.AddDacFx();

// Build DacPac from assembly
dacPacBuilder.BuildDacPac(
    assemblyFileFramework: "MyAssembly.dll",
    assemblyPdbFramework: "MyAssembly.pdb",
    dacpacFile: "output.dacpac",
    projectName: "MyProject",
    projectVersion: "1.0.0");
```

## License

See repository license file for details.

## Repository

[OoBDev Repository](https://github.com/yourusername/oobdev)
