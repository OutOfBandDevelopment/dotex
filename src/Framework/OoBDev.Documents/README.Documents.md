# OoBDev.Documents

## Overview

The OoBDev.Documents library provides functionality for managing documents, including storage, conversion, and type
detection. It offers classes and factories for creating and interacting with blob containers, document conversion 
services, and document type tools. These components enable developers to store, convert, and identify document types 
in their applications.

## Key Classes and Features

- **BlobContainerFactory**: Creates blob containers for storing documents.
- **FileSystemBlobContainerProvider**: a directory per container under `FileSystemBlobOptions.RootDirectory`; content type and metadata sit in a `.blobmeta.json` file beside each file; paths leaving the container directory are rejected. Register with `TryAddFileSystemBlobServices(configuration)`. Other providers: Azure (`OoBDev.Microsoft.Azure.StorageAccount`) and S3 (`OoBDev.Amazon.S3`).
- **DocumentConversion**: Performs document conversion from one format to another.
- **DocumentConversionChainBuilder**: Constructs document conversion chains.
- **DocumentTypeTools**: Provides tools for managing document types, including content type detection.
- **ServiceCollectionExtensions**: Extension methods for configuring document-related services in dependency injection.

Conversion handlers (IDocumentConversionHandler) live in their own libraries: [OoBDev.Documents.Hpgl](../OoBDev.Documents.Hpgl/README.Documents.Hpgl.md) (HP-GL to SVG) and [OoBDev.Documents.ExternalTools](../OoBDev.Documents.ExternalTools/README.Documents.ExternalTools.md) (a registered external tool such as Ghostscript or GhostPCL).


## Usage Example

Here's a simple example demonstrating how to use OoBDev.Documents to perform document conversion:

```csharp
using OoBDev.Documents.Conversion;
using OoBDev.Documents.DocumentTypeTools;

// Initialize document conversion chain builder
var conversionChainBuilder = new DocumentConversionChainBuilder();

// Add document conversion steps for converting from PDF to Word
var steps = conversionChainBuilder.Steps("application/pdf", "application/msword");

// Initialize document conversion service
var documentConversion = new DocumentConversion(conversionChainBuilder);

// Perform document conversion asynchronously
await documentConversion.ConvertAsync(sourceStream, "application/pdf", destinationStream, "application/msword");
```

## Malware scanning before storage

`ScanningBlobContainer` decorates any `IBlobContainer` and calls an `IContentScanner` (contract in `OoBDev.Documents.Abstractions`, namespace `OoBDev.Documents.Scanning`) before `StoreContentAsync`. An infected result or a scanner failure throws `ContentRejectedException` and nothing is stored (fail closed). Non-seekable streams are buffered once so the scan and the store read the same bytes. Reads, queries and deletes pass through. No scanner adapter ships yet (a ClamAV adapter would implement `IContentScanner`); wrap a container with `new ScanningBlobContainer(inner, scanner)` in the composition root.

Tests: `ScanningBlobContainerTests`.
