# Feature request: document converter dependencies and Linux support

- **Status:** done (built 2026-11-17, see [change](../../changes/2026-11-17.md#document-converter-dependencies-and-linux-support))
- **Requested by:** owner (source `TODO` comments, collected 2026-11-17)
- **Date:** 2026-11-17
- **Area:** `OoBDev.WkHtmlToPdf`, `OoBDev.HtmlToOpenXml`, document conversion tests, example Web API

## Need

The HTML to PDF (wkhtmltopdf) and HTML to OpenXml converters are registered with the common services, so every host pays for them. The owner wants either Tika instead or a way to exclude them. Some conversion tests need more configuration on Linux, and the example Web API has no run profile that disables hosting.

## Proposed behavior

Converters are opt-in registrations, not part of the common set; decide whether Tika replaces them (see the document services design). The Linux conversion tests run in the Docker stack (owner rule: not host installs). A launch profile runs the example Web API without its hosted services.

## Acceptance criteria

- Given the common registration, when a host is built, then no converter is registered unless added explicitly.
- Given the Docker stack, when the Linux-only conversion tests run, then they pass or end Inconclusive with a reason.
- Given the new profile, when the example starts, then no hosted service runs.

## Existing code checked

`docs/design/DocumentServices/ConversionService` (Tika based), request [document packaging and store](../completed/2026-11-10-document-packaging-and-store.md). They cover Tika but not removing the two converters from the common set.

Source comments (replaced by a reference to this request):

- `src/ExternalServices/WkHtmlToPdf/OoBDev.WkHtmlToPdf/ServiceCollectionExtensions.cs`: "consider switching to TIKA or at the very least making it so this can be excluded from common".
- `src/ExternalServices/HtmlToOpenXml/OoBDev.HtmlToOpenXml/ServiceCollectionExtensions.cs`: the same.
- `src/Examples/OoBDev.Example.Tests/Documents/IDocumentConversionTests.cs`: "these require more configuration on linux at this time".
- `src/Examples/OoBDev.Example.WebApi/Program.cs`: "add a run profile that disable hosting".
- `src/ExternalServices/WkHtmlToPdf/OoBDev.WkHtmlToPdf.Tests/HtmlToPdfConversionHandlerTests.cs`: "fix this better".

## Security and operations

wkhtmltopdf runs an external process on untrusted HTML; keep it opt-in and sandboxed.

## Priority

- **Impact:** needless dependencies in every host.
- **Needed by:** no date.
- **Workaround today:** none.

## Open questions

Replace with Tika or keep opt-in? Recommendation: opt-in now, evaluate Tika in the document services work.

## Triage notes

Triaged 2026-11-17 into [BACKLOG.md](../../../BACKLOG.md) section 5 (source TODO comments).

## Outcome

Built:

- `ExternalExtensionBuilder.IncludeWkHtmlToPdf` and `IncludeHtmlToOpenXml` (default false) gate the two converters in `TryCommonExternalExtensions`; Apache Tika stays registered and converts to PDF. The document converter CLI opts in to both because it advertises HTML to PDF.
- The example Web API reads `DISABLE_HOSTING` and has the launch profile `http (no hosting)`; it turns off the communications, message queue and vector hosted services.
- The wkhtmltopdf test ends Inconclusive with the reason (the bundled native library is Windows only); the commented HTML to PDF example rows now point to the Tika Integration rows.

Declined: replacing the converters with Tika outright (the HTML converters are in-process and useful for a host without Tika; the opt-in covers the exclusion need, and the Tika evaluation stays with the document services work).

Tests: 2 Unit tests (`ConverterRegistrationTests`).
