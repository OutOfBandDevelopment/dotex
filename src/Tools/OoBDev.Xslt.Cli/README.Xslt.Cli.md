# OoBDev.Xslt.Cli

A dotnet tool (`oobdev-xslt`) that runs an XSLT template over files of many types. Template file functions are limited to a sandbox folder.

## Description

Source: BinaryDataDecoders `Xslt.Cli`.

```
oobdev-xslt -i <input> -t <template.xslt> -o <output> [-e <exclude>] [-x <input-type>] [-s <sandbox>] [-m]
```

| Switch | Meaning |
|--------|---------|
| `-i`, `--input` | Input file or wildcard (required) |
| `-t`, `--template` | XSLT file (required) |
| `-o`, `--output` | Output pattern such as `out/*.html` (one file per input), or one file with `--merge` (required) |
| `-e`, `--exclude` | Wildcard of inputs to skip |
| `-x`, `--input-type` | `Xml` (default), `Html`, `Json`, `Yaml`, `CSharp`, `VB`, `MSBuildStructuredLog`, `DacPac`, `Path`, `ByExtension` |
| `-s`, `--sandbox` | Folder the template may read and write; defaults to the folder of the output |
| `-m`, `--merge` | Merge all inputs into one document before transforming |

`SandboxedTransformer` is public so other tools can reuse the input-type mapping.

## Verified

Unit tests cover option parsing, the extension and type mapping, single, JSON and merged transforms. The `ByExtension` name replaces the source's misspelt `ByExtention`.
