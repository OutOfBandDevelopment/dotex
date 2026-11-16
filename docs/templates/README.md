# Document templates

## Summary

Templates that `OoBDev.TemplateEngine.Cli` runs to turn tool output into markdown. They are inputs, not documents: nothing in this folder is generated, and generated output is not committed here. Output goes to `src/TestResults/docs/` (ignored by git) unless a feature request decides to commit a report.

## Contents

- [Templates](#templates)
- [Running one](#running-one)
- [Status](#status)

### List of Tables

1. [Table 1 — Templates](#table-1)

## Templates

<a id="table-1"></a>**Table 1 — Templates**

| Template | Engine | Input | Output | Wanted feature |
|----------|--------|-------|--------|----------------|
| `Documentation.md.xslt` | XSLT | compiler XML doc file (`*.xml`) | API reference page per assembly | [API reference](../features/request/2026-11-10-api-reference-from-xml-docs.md) |
| `Service-Endpoints.md.hbs` | Handlebars | OpenAPI JSON (with `x-permissions`) | endpoint, rights and request listing | [Endpoint listing](../features/request/2026-11-10-readme-index-and-endpoint-listing.md) |
| `SoftwareBillOfMaterials.md.xslt` | XSLT | CycloneDX 1.6 XML | component inventory | built 2026-11-16 by `scripts/docs/sbom.ps1`; [SBOM report](../features/request/2026-11-10-sbom-report.md) |
| `TestResultsToMarkdown.md.xslt` | XSLT | `.trx` test results | totals, failures and skipped tests | [Test results summary](../features/request/2026-11-10-test-results-summary.md) |
| `DuplicateXml.xml.xslt` | XSLT | any XML | identity copy, a starting point for new XSLT | none |

## Running one

The launch profiles of `OoBDev.TemplateEngine.Cli` (`src/Tools/OoBDev.TemplateEngine.Cli/Properties/launchSettings.json`) show the command lines. A template ending in `.xslt` runs through the sandboxed XSLT transformer, any other name through Handlebars. Switches: [README.TemplateEngine.Cli.md](../../src/Tools/OoBDev.TemplateEngine.Cli/README.TemplateEngine.Cli.md).

```text
dotnet run --project src/Tools/OoBDev.TemplateEngine.Cli -- --input <file> --output <file> --template <name> --file-template-path docs/templates
```

## Status

The test results template is built and tested (`TestResultsTemplateTests`: totals, per-tier table, failed, inconclusive and skipped lists); `scripts/ci/run-tests.ps1` runs it after every test run (one summary per `.trx` in `artifacts/test-results/summary`, not committed). The other templates are kept but their pipelines are not wired: no script runs them, and the old committed outputs (`docs/Libraries`, `docs/Service-Endpoints.md`, `docs/swagger.*`, `docs/Tests/Summary.md`) were deleted on 2026-11-10 because they were stale. The OpenAPI document now comes from `/openapi/{name}.json` (Scalar), not `swagger.json`, so `Service-Endpoints.md.hbs` may need a field check. The feature requests above decide which pipelines come back.
