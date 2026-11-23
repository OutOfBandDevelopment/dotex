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

## Choosing the provider per container

`TryAddBlobContainerSelection(configuration)` binds `BlobContainerOptions`; `Providers` maps a container name to a provider key (`file-system`, `aws-s3`, `azure-blob`). Each provider package registers its factory under that key as well as unkeyed. A container that is not listed keeps the default resolution; a listed container whose key is not registered fails with the container and key names.

```json
{ "BlobContainerOptions": { "Providers": { "invoices": "aws-s3", "exports": "file-system" } } }
```

## Packaging documents

`IDocumentPackager.PackAsync(type, items, stream)` writes several documents into one package; the handler is chosen by package type (case ignored). `TryAddDocumentServices` registers the `zip` handler (`ZipDocumentPackageHandler`, no password: the framework ZIP classes do not encrypt). Add another format by registering an `IDocumentPackageHandler` with its own `PackageType`. Entry names must be relative with forward slashes: an empty, rooted, backslash, drive, `..` or repeated (ignoring case) name throws `PackagingException` before anything is written, as does an unknown type. The originals are not changed. `DirectoryPackageItems.ReadAsync(folder)` turns a folder into items. Not built from the design: passwords and compression levels (see [PackingService](../../../docs/design/DocumentServices/PackingService/api-design.md)); the document store half of the [request](../../../docs/features/completed/2026-11-10-document-packaging-and-store.md) is open.

## Document store

`IDocumentStore.StoreAsync(content)` validates the document with `IValidateContent` (default `BypassContentValidation`; throw `InvalidContentException` to refuse, and nothing is stored), names it with `IDocumentKeyGenerator` (default `yyyy/MM/{guid}{ext}` in UTC from `TimeProvider`) and writes it to the registered `IBlobContainer`; `ReadAsync(key)` returns it or null. The validator may read the stream: the store rewinds a seekable stream afterwards. Register the container yourself (`IBlobContainer`); `TryAddDocumentServices` adds the rest.

`IDocumentResolver` does the same by container name through `IBlobContainerFactory`: `StoreAsync(container, content)` returns a `DocumentReference(Container, Key)` that a request can carry instead of the bytes, and `ResolveAsync(reference)` reads it back (null when missing).
