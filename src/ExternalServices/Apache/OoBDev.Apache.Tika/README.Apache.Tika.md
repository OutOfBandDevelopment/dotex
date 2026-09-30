# OoBDev.Apache.Tika


## Summary

The OoBDev.Apache.Tika library provides functionality for content type detection and document conversion 
using Apache Tika. It offers a set of classes and methods for integrating Apache Tika services with .NET 
applications.

## Installation

To use this library in your .NET project, add a reference to the OoBDev.Apache.Tika NuGet package.

## Usage

### Content Type Detection

The `TikaContentTypeDetector` class provides methods for asynchronously detecting the content type of a 
stream using Apache Tika.

```csharp
using OoBDev.Apache.Tika;

// Detect content type asynchronously
string contentType = await TikaContentTypeDetector.DetectContentTypeAsync(stream);


## Document Conversion

The library includes several conversion handlers for converting documents to HTML format using Apache 
Tika. Each handler supports specific document formats.

### Word Documents (DOCX)

```csharp
using OoBDev.Apache.Tika;

// Convert DOCX document to HTML
TikaDocxToHtmlConversionHandler handler = new TikaDocxToHtmlConversionHandler();
handler.ConvertAsync(sourceStream, sourceContentType, destinationStream, destinationContentType);
```

### PDF Documents

```csharp
using OoBDev.Apache.Tika;

// Convert PDF document to HTML
TikaPdfToHtmlConversionHandler handler = new TikaPdfToHtmlConversionHandler();
handler.ConvertAsync(sourceStream, sourceContentType, destinationStream, destinationContentType);
```

### OpenDocument Text (ODT) Documents

```csharp
using OoBDev.Apache.Tika;

// Convert ODT document to HTML
TikaOdtToHtmlConversionHandler handler = new TikaOdtToHtmlConversionHandler();
handler.ConvertAsync(sourceStream, sourceContentType, destinationStream, destinationContentType);
```

### Rich Text Format (RTF) Documents

```csharp
using OoBDev.Apache.Tika;

// Convert RTF document to HTML
TikaRtfToHtmlConversionHandler handler = new TikaRtfToHtmlConversionHandler();
handler.ConvertAsync(sourceStream, sourceContentType, destinationStream, destinationContentType);
```

## Extensibility

Developers can extend the functionality by inheriting from `TikaToHtmlConversionBaseHandler` or 
`TikaConversionHandlerBase` classes for custom document conversion requirements.
