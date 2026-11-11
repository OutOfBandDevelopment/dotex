# OoBDev.Documents.Abstractions

Contracts for blob storage, document conversion and document type detection. Implementations: [OoBDev.Documents](../OoBDev.Documents/README.Documents.md) (factory, conversion, type tools), the Azure storage account adapter (blob provider) and the conversion adapters (Markdig, WkHtmlToPdf, HtmlToOpenXml, MysticMind).

**Table 1 — Contracts**

| Type (namespace) | Role |
|------------------|------|
| `IBlobContainer` (`OoBDev.Documents`) | `GetContentAsync(path)`, `GetContentMetaDataAsync(path)`, `StoreContentAsync(reference, metadata, overwrite)`, `StoreContentMetaDataAsync`, `QueryContent()` (an `IQueryable<ContentMetaDataReference>`) and `DeleteContentAsync(path)`. `IBlobContainer<T>` is the same contract bound to a type |
| `BlobContainerAttribute` | Class attribute with `ContainerName`, naming the container a type uses |
| `IBlobContainerFactory` (`.Containers`) | `Create(containerName)` and `Create<T>()` |
| `IBlobContainerProvider` | A container for one provider technology, with a settable `ContainerName` |
| `IBlobContainerProviderFactory` | `Create(containerName)` returns a provider, or null when it does not serve that container |
| `IDocumentConversion` | `ConvertAsync(source, sourceType, destination, destinationType)` returns false when no handler supports the pair |
| `IDocumentConversionHandler` (`.Conversion`) | One conversion: `Sources`, `Destinations`, `SupportedSource`, `SupportedDestination` and `ConvertAsync`; handlers throw `NotSupportedException` for an unsupported type |
| `IContentTypeDetector` | `DetectContentTypeAsync(stream)` returns the content type or null |
| `IDocumentTypeTools` | Content type detection plus lookup of an `IDocumentType` by content type, file extension or file header stream |

**Table 2 — Models (namespace `OoBDev.Documents.Models`)**

| Type | Members |
|------|---------|
| `ContentReference` | `Content` (stream), `ContentType`, `FileName` (all required) |
| `ContentMetaDataReference` | `ContentType`, `FileName` (required), optional `MetaData` dictionary |
| `IDocumentType`, `DocumentType` | `Name`, `ContentTypes`, `FileExtensions`, `FileHeader` (bytes); the record is the default implementation adapters register |

## Use

A conversion adapter implements the handler and registers its document types:

```csharp
services.AddTransient<IDocumentConversionHandler, MyHandler>();
services.AddTransient<IDocumentType>(_ => new DocumentType
{
    Name = "Markdown", FileHeader = [], FileExtensions = [".md"], ContentTypes = ["text/markdown"],
});
```

Consumers inject `IDocumentConversion` (the framework picks the handler whose sources and destinations match) and `IBlobContainerFactory`.
