# OoBDev.DacPacCompiler.Cli

Command-line tool that builds a SQL Server DacPac from a SQL CLR assembly (the .NET Framework 4.8 build of a project such as `OoBDev.Data.Vectors`). The tool command is `dacpactools` when installed as a dotnet tool.

## Contents

- [Installation](#installation)
- [Usage](#usage)
- [Not wired yet: merging DacPacs](#not-wired-yet-merging-dacpacs)

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
| `--tool` | no | `SqlClr` (default), `Merge` or `Unknown`; see below. |

The host binds the options from the command line, logs them and calls `IDacPacBuilder.BuildDacPac` (`OoBDev.DacFx`), then exits. The generated reference page is [oobdev-dacpac](../../../docs/applications/tools/oobdev-dacpac.md).

## Not wired yet: merging DacPacs

`DacPacMergeTemplate.yml` and the `Merge` value of `--tool` come from an earlier version that also merged several DacPacs into one. Today the host registers only `TryAddDacPacServices()` and the builder service, so `--tool Merge` does nothing. The merge itself exists as `IDacpacMergeCompiler` in `OoBDev.Microsoft.SqlServer.DacFx` (`AddDacPacCompilerServices()`); wiring it into this tool is tracked in `BACKLOG.md`.
