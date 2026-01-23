# OoBDev.Data.Vectors.Net481

.NET Framework 4.8 build target for OoBDev.Data.Vectors SQL CLR deployment.

## Description

This package provides the .NET Framework 4.8.1 compiled version of OoBDev.Data.Vectors required for SQL Server CLR deployment. It shares the same source code as OoBDev.Data.Vectors but targets the .NET Framework runtime required by SQL Server.

## Key Features

- .NET Framework 4.8.1 target for SQL CLR
- Shared source with OoBDev.Data.Vectors
- DacPac generation support
- SQL Server CLR safe mode compatible

## Installation

This package is typically used as part of a database project build process rather than directly referenced:

```xml
<ProjectReference Include="OoBDev.Data.Vectors.Net481.csproj" />
```

## Usage

This project automatically builds as part of the OoBDev.Data.Vectors.DB database project to generate the SQL CLR assembly and DacPac for deployment to SQL Server.

## License

See repository license file for details.

## Repository

[OoBDev Repository](https://github.com/yourusername/oobdev)
