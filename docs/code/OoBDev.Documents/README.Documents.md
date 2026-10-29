# OoBDev.Documents

## Overview

The OoBDev.Documents library provides functionality for managing documents, including storage, conversion, and type
detection. It offers classes and factories for creating and interacting with blob containers, document conversion 
services, and document type tools. These components enable developers to store, convert, and identify document types 
in their applications.

## Key Classes and Features

- **BlobContainerFactory**: Creates blob containers for storing documents.
- **DocumentConversion**: Performs document conversion from one format to another.
- **DocumentConversionChainBuilder**: Constructs document conversion chains.
- **DocumentTypeTools**: Provides tools for managing document types, including content type detection.
- **ServiceCollectionExtensions**: Extension methods for configuring document-related services in dependency injection.


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