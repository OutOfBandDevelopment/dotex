# OoBDev - Template Engine

## Summary

This tool is a dotnet tool that can be used to transform various files into text 
documents based on provided templates

## Handlebars and XSLT templates

The template decides the path. A template name goes to the template engine (Handlebars) with JSON or XML input. A template that ends in `.xslt` or `.xsl` runs through `SandboxedTransformer` from `OoBDev.System.Text.Templating.Xslt`, the transformer formerly in `OoBDev.Xslt.Cli`.

| Switch | Use |
|--------|-----|
| `--input` | required; input file; a wildcard in the file name selects several files |
| `--input-type` | `Json` or `Xml` for Handlebars (default: from the file extension `.json` or `.xml`; any other input throws `NotSupportedException`); any `InputTypes` value for XSLT (`Xml`, `Html`, `Json`, `Yaml`, `CSharp`, `VB`, `MSBuildStructuredLog`, `DacPac`, `Path`, `ByExtension`; default `ByExtension`) |
| `--template` | required; template name, or the XSLT file |
| `--output` | required; output file; `[file]` is the input name without extension |
| `--exclude` | XSLT only: wildcard of inputs to skip |
| `--sandbox` | XSLT only: the folder the template may read and write (default: the output folder) |
| `--merge true` | XSLT only: merge all inputs into one document first |

Tests: `OoBDev.TemplateEngine.Cli.Tests` (Unit). Plan: [template tools](../../../docs/design/TemplateTools/migration-plan.md).
