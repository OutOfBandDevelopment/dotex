# Feature request: embedded resource fallback for templates

- **Status:** done (built 2026-11-17, see [change](../../changes/2026-11-17.md#embedded-template-source))
- **Requested by:** owner (open gap found reviewing the migration, 2026-11-10)
- **Date:** 2026-11-10
- **Area:** `OoBDev.System.Text.Templating*`, `OoBDev.Handlebars`, `OoBDev.TemplateEngine.Cli`

## Need

Templates are loaded from files only, so a library cannot ship a default template that users override.

## Proposed behavior

Look up a template by name and file type: the caller's folder first, then an embedded resource of the library.

## Acceptance criteria

- Given a template present on disk, then it wins over the embedded one.
- Given none on disk, then the embedded template for that file type is used.

## Existing code checked

`OoBDev.TemplateEngine.Cli`, the `OoBDev.Handlebars` provider and the XSLT transformer.

## Security and operations

Embedded resources are trusted; disk templates follow the existing sandbox rules.

## Priority

- **Impact:** default templates ship with packages.
- **Needed by:** no date
- **Workaround today:** none

## Open questions

Where does the lookup live? A template source abstraction shared with the SharedFramework Epic 10 design would fit.

## Triage notes

Origin: BinaryDataDecoders issue 27. Accept, schedule or decline.

2026-11-11: triaged. Accepted in principle: a template source that falls back to an embedded resource, added to [TextTemplating](../../design/TextTemplating/README.md) as a source option. Scheduled behind template persistence. Backlog: BACKLOG.md, SharedFramework.

## Outcome

`EmbeddedResourceTemplateSource` (`OoBDev.System`) finds `prefix + name + target extension + template extension` among an assembly's manifest resources (folders in the name become dots, case-insensitive) with priority 1000. `TemplateEngine` takes the lowest priority first and the file source uses 100, so a template on disk wins and the embedded default is the fallback. Registered with `AddEmbeddedTemplates(assembly, prefix)`. The lookup lives in the shared `ITemplateSource` abstraction, so the Handlebars and XSLT providers and `OoBDev.TemplateEngine.Cli` get it without change. Unit tests: 2.
