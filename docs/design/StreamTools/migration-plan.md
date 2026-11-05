# Migration plan: external conversion onto the document conversion framework

[StreamTools](README.md) | Status: **in progress, built on the owner answer to Q2 (plan not yet formally confirmed)**; done: `OoBDev.Documents.Hpgl` (handler, moved converter and tests, DevTerm references it); `OoBDev.Documents.ExternalTools` (handler, options, content-type map, tests); left: the DevTerm adapter and deleting the moved code (answer Q2, 2026-11-04: option A, the framework `IDocumentConversion` pattern; DevTerm uses it)

Today `StreamCaptureConverter` in `OoBDev.DevTerm.Core/Configuration` runs registered tools (Ghostscript, GhostPCL) and the built-in HP-GL to SVG converter. The framework has `IDocumentConversionHandler` (`OoBDev.Documents.Abstractions`) and `DocumentConversionChainBuilder` (`OoBDev.Documents`). Handlers take streams and content types, so a handler that needs files writes temp files.

**Table 1 — Moves**

| Item | Current location | Target location | Namespace | Package or unit |
|------|------------------|-----------------|-----------|-----------------|
| `HpglToSvgConverter` plus an `IDocumentConversionHandler` wrapper (`image/vnd.hp-hpgl` to `image/svg+xml`) | `OoBDev.DevTerm.Core/StreamContent` | new `src/Framework/OoBDev.Documents.Hpgl` | `OoBDev.Documents.Hpgl` | New Framework project, packable |
| `ExternalToolConversionHandler` (runs a registered tool: name, path, argument template with `{input}` `{output}` `{dpi}`, formats, output extension, dpi; one handler per registered tool; no shell, `ArgumentList` as today) | `StreamCaptureConverter.RunToolAsync` | new `src/Framework/OoBDev.Documents.ExternalTools` (keeps process execution out of `OoBDev.Documents`) | `OoBDev.Documents.ExternalTools` | New Framework project, packable |
| `ExternalToolOptions` (was `StreamConvertToolOptions`) | `CliOptions.cs` | `OoBDev.Documents.ExternalTools` | same | same |
| `StreamCaptureConverter`, `StreamConversionMode`, `StreamConversionResult`, tool-list store and editor, `Handles` format matching to content types | `OoBDev.DevTerm.Core/Configuration` | stay; the converter becomes a thin adapter that maps a capture kind to a content type and calls `IDocumentConversion` | `OoBDev.DevTerm.Configuration` | DevTerm |
| Tests | `HpglToSvgConverterTests`, converter tests in `DevTerm.Core.Tests` | handler tests move to `OoBDev.Documents.Hpgl.Tests` and `OoBDev.Documents.ExternalTools.Tests` (fake tool, `Unit`); DevTerm keeps adapter tests | - | MSTest |

**Table 2 — References that change**

| Project | Change |
|---------|--------|
| `OoBDev.DevTerm.Core` | references `OoBDev.Documents`, `.Hpgl`, `.ExternalTools`; registers handlers with `TryAdd*` |
| `OoBDev.Documents.ExternalTools`, `.Hpgl` | reference `OoBDev.Documents.Abstractions` only |
| Readmes | `README.Documents.ExternalTools.md`, `README.Documents.Hpgl.md`; update `README.Documents.md` and `stream-converter-tools.md` |

Open design point (decided in the plan, change if you disagree): the saved-file output path next to the capture stays DevTerm's job; the handler writes to the destination stream, and the adapter saves it.

Order: handlers and tests first, then the DevTerm adapter, then delete the moved code, then run the DevTerm tests and the conversion journeys.
