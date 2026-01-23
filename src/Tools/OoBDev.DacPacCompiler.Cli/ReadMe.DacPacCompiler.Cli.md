# OoBDev.DacPacCompiler.Cli

Command-line tool for compiling .NET assemblies into SQL Server DacPac files.

## Description

This CLI tool compiles .NET Framework 4.8 assemblies containing SQL CLR types into SQL Server DacPac files. It extracts SQL CLR metadata and generates deployment packages for SQL Server.

## Key Features

- .NET assembly to DacPac compilation
- SQL CLR metadata extraction
- PDB symbol file inclusion
- Project version and naming support
- Hosted service architecture
- DacPac validation

## Installation

```xml
<PackageReference Include="OoBDev.DacPacCompiler.Cli" Version="*" />
```

Or install as a global tool:

```bash
dotnet tool install -g OoBDev.DacPacCompiler.Cli
```

## Basic Usage

```bash
dacpactools --sqlclr "assembly.dll" --dotnet "assembly.dll" --output "output.dacpac" --name "MyProject" --version "1.0.0"
```

Command-line arguments:
- `--sqlclr` - Path to .NET Framework 4.8 assembly
- `--dotnet` - Path to .NET assembly (for reference)
- `--output` - Output DacPac file path
- `--name` - Project name
- `--version` - Project version

## License

See repository license file for details.

## Repository

[OoBDev Repository](https://github.com/yourusername/oobdev)
