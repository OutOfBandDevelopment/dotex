# OoBDev.Apache.Tika

Content type detection and document-to-HTML conversion through an Apache Tika server, plugged into the `OoBDev.Documents` abstractions.

## Contents

- [Registration](#registration)
- [Configuration](#configuration)
- [Content type detection](#content-type-detection)
- [Document conversion](#document-conversion)
- [Health check](#health-check)
- [Verification](#verification)

## Registration

```csharp
services.TryAddApacheTikaServices(configuration);   // section name is required in DEBUG builds
```

Nothing is registered when the `ApacheTikaClientOptions:Url` setting is missing, so a host without Tika simply has no Tika services. When it is set the call registers:

- `IApacheTikaClient` (typed `HttpClient` with `BaseAddress` set to the URL),
- `TikaContentTypeDetector` as `IContentTypeDetector`,
- one `IDocumentConversionHandler` per format and one `IDocumentType` per format,
- the `apache-tika` health check, and `ApacheTikaClientOptions` validated with data annotations.

## Configuration

```json
{ "ApacheTikaClientOptions": { "Url": "http://localhost:9998" } }
```

**Table 1 — Options**

| Key | Required | Use |
|-----|----------|-----|
| `Url` | yes | Base URL of the Tika server. The test stack publishes it on port 9998 (`TIKA_URL` in `src/.runsettings`). |

## Content type detection

`TikaContentTypeDetector.DetectContentTypeAsync(Stream)` sends the stream to the server (`PUT /detect`, then `PUT /detect/stream` when the first call does not answer) and returns the detected MIME type.

```csharp
IContentTypeDetector detector = provider.GetRequiredService<IContentTypeDetector>();
string? contentType = await detector.DetectContentTypeAsync(stream);
```

## Document conversion

Each handler converts one family of sources to `text/html` by calling the server (`/tika/html`; `text/plain` and `text/xml` are also mapped by the client). Pick the handler through the conversion service of `OoBDev.Documents`, or call a handler directly.

**Table 2 — Handlers and document types**

| Handler | Format | Extension |
|---------|--------|-----------|
| `TikaDocToHtmlConversionHandler` | Microsoft Word | `.doc` |
| `TikaDocxToHtmlConversionHandler` | Microsoft Word, OpenXML | `.docx` |
| `TikaPdfToHtmlConversionHandler` | Portable Document Format (header `%PDF-`) | `.pdf` |
| `TikaOdtToHtmlConversionHandler` | Open Office document | `.odt` |
| `TikaEpubToHtmlConversionHandler` | Electronic publication | `.epub` |
| `TikaRtfToHtmlConversionHandler` | Rich Text Format | `.rtf` |
| `TikaXlsToHtmlConversionHandler` | Microsoft Excel (binary) | `.xls` |
| `TikaPptToHtmlConversionHandler` | Microsoft PowerPoint (binary) | `.ppt` |

```csharp
await handler.ConvertAsync(source, sourceContentType, destination, "text/html");
```

A source or destination content type the handler does not list throws `NotSupportedException`. To add a format, derive from `TikaToHtmlConversionBaseHandler` (HTML output) or `TikaConversionHandlerBase` (other outputs), expose the `SOURCES` content types and register the handler as an `IDocumentConversionHandler`.

## Health check

`ApacheTikaHealthCheck` calls the server root (`GET /tika`): the answer is reported as healthy, and any exception as degraded.

## Verification

`OoBDev.Apache.Tika.Tests` has Unit tests for the health check, detector and handlers, and Integration tests against the Tika container of the test stack (see `docs/architecture/testing/integration/`).
