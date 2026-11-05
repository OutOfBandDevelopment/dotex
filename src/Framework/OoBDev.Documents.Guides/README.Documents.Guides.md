# OoBDev.Documents.Guides

## Overview

Deterministic markdown guides generated from machine-readable specifications, for the generated-guides workflow ([design](../../../docs/design/GeneratedGuides/README.md)). The same input always gives the same bytes, so reruns leave git alone.

## Key Classes

- **ApiGuideRenderer**: `OpenApi(json, title)` and `AsyncApi(json, title)` render an OpenAPI or AsyncAPI document as markdown (operations sorted by path, channels with their parameters and messages); `WriteIfChanged(path, content)` writes LF text only when the file differs.

## Usage

```csharp
var guide = ApiGuideRenderer.OpenApi(openApiJson, "My service: REST API");
ApiGuideRenderer.WriteIfChanged("docs/applications/my-service/api-guide/rest.md", guide);
```

## Dependencies

BCL only (`System.Text.Json`); no test framework and no web host, so any test or tool project can reference it.

## Not here

The Kaitai (`.ksy`) guide renderer stays with DevTerm because it reads the DevTerm importer's model (`KsyImporter`). The command-line tool guide helper is planned ([migration plan](../../../docs/design/GeneratedGuides/migration-plan.md)).
