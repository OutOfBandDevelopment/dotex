# OoBDev.DacPacCompiler.Cli

Command-line tool that builds a SQL Server DacPac from a SQL CLR assembly (the .NET Framework 4.8 build of a project such as `OoBDev.Data.Vectors`) or merges several DacPacs into one. The tool command is `dacpactools` when installed as a dotnet tool.

## Contents

- [Installation](#installation)
- [Usage](#usage)
- [Merging DacPacs](#merging-dacpacs)

## Installation

```bash
dotnet tool install -g OoBDev.DacPacCompiler.Cli
```

## Usage

```bash
dacpactools --sqlclr "OoBDev.Data.Vectors.dll" --pdb "OoBDev.Data.Vectors.pdb" --dacpac "OoBDev.Data.Vectors.dacpac" --project "OoBDev.Data.Vectors" --version "1.0.0"
```

**Table 1 — Options** (`--help` prints them; pass each as `--name value`)

| Option | Required | Use |
|--------|----------|-----|
| `--sqlclr` | yes | Path of the .NET Framework SQL CLR assembly to read (its metadata is inspected, it is not executed). |
| `--pdb` | no | Symbol file to include; defaults to the assembly path with `.pdb`. |
| `--dacpac` | no | Output file; defaults to the assembly path with `.dacpac`. |
| `--project` | no | Project name for the package metadata; defaults to the assembly file name. |
| `--version` | no | Project version for the package metadata; defaults to `0.0.0.1`. |
| `--tool` | no | `SqlClr` (default) or `Merge` (see [Merging DacPacs](#merging-dacpacs)). |

The host binds the options from the command line, logs them and calls `IDacPacBuilder.BuildDacPac` (`OoBDev.DacFx`), then exits. The generated reference page is [oobdev-dacpac](../../../docs/applications/tools/oobdev-dacpac.md).

## Merging DacPacs

`--tool Merge` merges several DacPacs into one package with `IDacpacMergeCompiler` (`OoBDev.Microsoft.SqlServer.DacFx`, `AddDacPacCompilerServices()`). The merge host reads its own switches, so `--sqlclr` is not needed and `--version` is the package version.

```bash
dacpactools --tool Merge --source-path ./dacpacs --source-patterns "*.dacpac;!*.Tests.dacpac" --target-path ./out/Merged.dacpac --version 1.2.3 --setting-modeloptionsource First
```

**Table 2 — Merge options** (`--help` lists them after the build options)

| Option | Short | Use |
|--------|-------|-----|
| `--template` | `-t` | Template file read through the object converter (JSON); the other options override it. `DacPacMergeTemplate.yml` is not read: YAML is still commented out in `TemplateFactoryBase`. |
| `--source-path` | `-s` | Folder the patterns are resolved against; defaults to the current folder. |
| `--source-patterns` | `-p` | Semicolon separated glob patterns, `!` excludes; defaults to `*.dacpac`. |
| `--target-path` | `-r` | The merged DacPac to write. |
| `--name`, `--description` | `-n`, `-d` | Package metadata; the name defaults to the target file name, the description to the local time. |
| `--version`, `--build-version` | `-v`, `-b` | Package version (default `0.0.0`) and the build version written by the post-deployment script (default: the version). |
| `--setting-modeloptionsource` | | `Custom`, `First` or `Last`: which source the model options come from. |

Pre- and post-deployment scripts of the sources are carried into the merged package. Tests: `OoBDev.DacPacCompiler.Cli.Tests` (tool selection and a merge of two generated packages).
