# OoBDev.Microsoft.SqlServer.DacFx

DacPac compilation and merging utilities for SQL Server projects.

## Description

This package provides tools for creating and merging SQL Server Data-tier Application packages (DacPac). It enables programmatic manipulation of DacPac files, including merging multiple packages and managing deployment scripts.

## Key Features

- DacPac file merging and compilation
- Pre and post-deployment script management
- Build version tracking
- Package metadata customization
- Template-based DacPac generation
- SQL Server versioning support
- Integration with Microsoft DacFx

## Installation

```xml
<PackageReference Include="OoBDev.Microsoft.SqlServer.DacFx" Version="*" />
```

## Basic Usage

```csharp
using OoBDev.Microsoft.SqlServer.DacFx;
using Microsoft.Extensions.DependencyInjection;

services.AddDacPacMergeCompiler(configuration);

// Create merge definition
var definition = factory.CreateDefinition(
    sourceFiles: new[] { "package1.dacpac", "package2.dacpac" },
    targetPath: "merged.dacpac",
    targetBuildVersion: "1.0.0");

// Compile merged package
compiler.CreatePackage(definition);
```

## License

See repository license file for details.

## Repository

[OoBDev Repository](https://github.com/yourusername/oobdev)
