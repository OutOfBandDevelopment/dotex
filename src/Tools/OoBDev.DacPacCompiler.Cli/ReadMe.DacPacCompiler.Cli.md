# OoBDev.DacPacCompiler.Cli

Command-line tool for working with SQL Server DacPac files.

## Description

This CLI tool provides two primary capabilities:
1. Compiling .NET Framework 4.8 assemblies containing SQL CLR types into SQL Server DacPac files
2. Merging multiple DacPac files into a single package

## Key Features

- .NET assembly to DacPac compilation
- SQL CLR metadata extraction
- DacPac merging and composition
- PDB symbol file inclusion
- Project version and naming support
- Template-based configuration
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

## Usage

### Compiling .NET Assembly to DacPac

```bash
dacpactools --sqlclr "assembly.dll" --dotnet "assembly.dll" --output "output.dacpac" --name "MyProject" --version "1.0.0"
```

Command-line arguments:
- `--sqlclr` - Path to .NET Framework 4.8 assembly
- `--dotnet` - Path to .NET assembly (for reference)
- `--output` - Output DacPac file path
- `--name` - Project name
- `--version` - Project version

### Merging Multiple DacPac Files

```bash
dacpactools -s "." -p "*.dacpac" -r "merged.dacpac" -d "Merged package" -n "MyDatabase" -v "1.2.3"
```

Command-line parameters:
- `-t | --template` - Path to a configuration template YAML file
- `-s | --source-path` - Base directory to be used with source patterns
- `-p | --source-patterns` - File globbing pattern to select DacPacs to be merged
- `-r | --target-path` - Full output path for merged DacPac results
- `-d | --description` - Description to be added to the DacPac metadata
- `-n | --name` - Name to be added to the DacPac metadata (defaults to output filename)
- `-v | --version` - Version number to apply to DacPac metadata
- `-b | --build-version` - Extended version number for build (defaults to version if not provided)

## Template File Example

```yml
SourcePath: "."
SourcePatterns:
- "*.dacpac"

TargetPath: composed_XXXXXXXXXX.dacpac
ServerVersion: SqlAzure

ModelOptionSource: Custom
ModelOptions: # Based on Microsoft.SqlServer.Dac.Model.TSqlModelOptions
- Property: Value

Name: test name
Description: test desc
Version: 1.2.3
BuildVersion: 20200527-1.2.3-XYZ.4
```

## License

See repository license file for details.

## Repository

[OoBDev Repository](https://github.com/mwwhited/dotex)
