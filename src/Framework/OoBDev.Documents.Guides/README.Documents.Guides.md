# OoBDev.Documents.Guides

## Overview

Deterministic markdown guides generated from machine-readable specifications, for the generated-guides workflow ([design](../../../docs/design/GeneratedGuides/README.md)). The same input always gives the same bytes, so reruns leave git alone.

## Key Classes

- **ApiGuideRenderer**: `OpenApi(json, title)` and `AsyncApi(json, title)` render an OpenAPI or AsyncAPI document as markdown (operations sorted by path, each with its access line from the `x-permissions` extension: anonymous or authenticated plus the required rights; channels with their parameters and messages); `WriteIfChanged(path, content)` writes LF text only when the file differs.
- **CliToolGuide**: `Render(title, synopsis, examples)` turns captured `CliExample` runs (title, description, command line, output, exit code) into a markdown guide with `console` blocks.
- **XmlDocReferenceRenderer**: `Render(xml)` turns a compiler XML documentation file into an API reference page (types in ordinal order with their documented fields, properties, methods and events; members that only inherit their docs are left out). `OoBDev.Docs.Tests` `ApiReference.Write` writes one page per project into the test's build output (`api-reference`) and removes pages of removed projects.
- **CliToolRunner**: `RunAsync(...)` starts a tool, captures stdout and stderr and returns a `CliExample`; `displayCommandLine` keeps machine paths out of the guide.

## Usage

```csharp
var guide = ApiGuideRenderer.OpenApi(openApiJson, "My service: REST API");
ApiGuideRenderer.WriteIfChanged("docs/applications/my-service/api-guide/rest.md", guide);
```

## Dependencies

BCL only (`System.Text.Json`); no test framework and no web host, so any test or tool project can reference it.

## Not here

The Kaitai (`.ksy`) guide renderer stays with DevTerm because it reads the DevTerm importer's model (`KsyImporter`). (See the [migration plan](../../../docs/design/GeneratedGuides/migration-plan.md)).
