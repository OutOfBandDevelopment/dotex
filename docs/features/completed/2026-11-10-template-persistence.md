# Feature request: template persistence (text template store)

- **Status:** done
- **Requested by:** owner (SharedFramework comparison, 2026-11-10)
- **Date:** 2026-11-10
- **Area:** `OoBDev.System.Text.Templating*`, `OoBDev.Handlebars` (Epic 10 Text Templating)

## Need

Save, list and resolve text templates by name from a store. dotex renders templates (`ITemplateEngine`, `ITemplateProvider`, `ITemplateSource`, the Handlebars provider) but its only template store behaviour is in the example web API.

## Proposed behavior

A template provider for stored templates (`ITextTemplateProvider`), template summary and template models, and a resolver that finds a template by name. The database-backed source is in the design.

## Source projects

Under `C:\repo\mine\proving-grounds\examples\SharedFramework` (read-only reference):

- `OoBDev.TextTemplating.Abstractions` (`IGenerateText`, `ITemplateResolver`, `ITextTemplateProvider`, `TemplateSummaryModel`, `TextTemplateModel`)
- `OoBDev.TextTemplating` (`GenerateText`, `TemplateResolver`, registrar)

## Acceptance criteria

- Given a template saved under a name, then it is listed and resolved by that name.
- Given two versions of a template, then the resolver returns the current one.
- Given a missing name, then the resolver falls back (see the embedded template fallback request) or returns a clear miss.

## Existing code checked

Rendering is covered. Only `OoBDev.Example.WebApi/Controllers/TextTemplateController.cs` touches template management. The design exists: [TextTemplating](../../design/TextTemplating/README.md) (`DatabaseTemplateSource`, `HandlebarsProvider`, `DataContainerIntegration`).

## Security and operations

Stored templates are user input: render them in the existing sandbox rules and limit helpers. Control who may edit them.

## Priority

- **Impact:** editable templates without a deploy.
- **Needed by:** no date (the epic waits for a consumer)
- **Workaround today:** templates as files

## Open questions

Does it share the lookup with the embedded template fallback request?

## Triage notes

Origin: SharedFramework. Design exists; no code until a consumer appears.

2026-11-19: done. Stored templates are listed and resolved by name and version: `EfTemplateSource` and `EfTemplateRepository` ([Template versioning](../../changes/2026-11-16.md#template-versioning), tenant override, rollback, culture), the fallback is [embedded templates](../../changes/2026-11-17.md#embedded-template-source), and Handlebars renders a stored row ([change](../../changes/2026-11-19.md#handlebars-templates-from-the-database)). The `IDataContainer` adapter and shipped migrations stay in BACKLOG.md.
2026-11-11: triaged. Design exists in [TextTemplating](../../design/TextTemplating/README.md) (`DatabaseTemplateSource`); no code until a consumer appears. Backlog: BACKLOG.md, SharedFramework.
