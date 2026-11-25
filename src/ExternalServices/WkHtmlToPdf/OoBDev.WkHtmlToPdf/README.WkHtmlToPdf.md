# OoBDev.WkHtmlToPdf

In-process HTML to PDF conversion for the document conversion pipeline (`IDocumentConversionHandler`), using the native wkhtmltopdf library through [Haukcode.WkHtmlToPdfDotNet](https://www.nuget.org/packages/Haukcode.WkHtmlToPdfDotNet).

## Use

```csharp
services.TryAddWkHtmlToPdfServices();
```

The common registration does not add it; set `ExternalExtensionBuilder.IncludeWkHtmlToPdf`. The bundled native library is Windows only, so the `DevLocal` test ends Inconclusive elsewhere.

This registers `HtmlToPdfConversionHandler`, a single shared `SynchronizedConverter` (the native library is not thread-safe, so conversions are serialized), the `PdfTools` and an `IDocumentType` for `application/pdf` (extension `.pdf`, header `%PDF-`).

## Behaviour

**Table 1 — `HtmlToPdfConversionHandler`**

| Item | Value |
|------|-------|
| Sources | `text/html`, `text/xhtml`, `text/xhtml+xml` (case-insensitive) |
| Destinations | `application/pdf` |
| Other content types | `NotSupportedException` |

`ConvertAsync` reads the whole source stream as text (the stream stays open) and renders it with the page settings of `HtmlToPdfOptions`: colour mode, orientation, paper size, default encoding and the header (right text, font size, line). The defaults are colour, portrait, Letter, UTF-8 and a header line with `Page [page] of [toPage]` on the right; change them with `services.Configure<HtmlToPdfOptions>(...)` or by binding a configuration section. `CreateDocument(html)` builds the wkhtmltopdf document without the native library. The native wkhtmltopdf binary must be loadable on the host; the code notes Tika as a possible replacement.

## Tests

`OoBDev.WkHtmlToPdf.Tests` has one `DevLocal` conversion test (it needs the native library on the machine); the content-type checks and the option-to-document mapping are `Unit` tests that need no native library.

## Notes

- https://wkhtmltopdf.org/
