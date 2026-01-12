# OoBDev.DacFx.Abstractions

Abstractions for SQL Server DacPac building and compilation.

## Description

This package provides abstractions and interfaces for working with SQL Server Data-tier Application packages (DacPac). It defines contracts for DacPac building, validation, merging, and template generation.

## Key Features

- IDacPacBuilder interface for DacPac creation
- IDacPacValidator for package validation
- IDacPacMergeCompiler for combining packages
- Template factory abstractions
- Model option source configuration
- SQL Server version targeting

## Installation

```xml
<PackageReference Include="OoBDev.DacFx.Abstractions" Version="*" />
```

## Basic Usage

```csharp
using OoBDev.DacFx;

// Implement custom DacPac builder
public class MyDacPacBuilder : IDacPacBuilder
{
    public void BuildDacPac(
        string assemblyFileFramework,
        string? assemblyPdbFramework = null,
        string? dacpacFile = null,
        string? projectName = null,
        string? projectVersion = null)
    {
        // Your implementation
    }
}
```

## License

See repository license file for details.

## Repository

[OoBDev Repository](https://github.com/yourusername/oobdev)
