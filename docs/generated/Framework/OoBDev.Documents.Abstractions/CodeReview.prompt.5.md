Review the follow source code as a senior software engineer / solutions architect.   
Suggest changes to coding patterns, methods, structure and class to make the code 
more maintainable.  

## Source Files

```IDocumentTypeTools.cs

using OoBDev.Documents.Models;
using System.IO;
using System.Threading.Tasks;

namespace OoBDev.Documents;

/// <summary>
/// Provides tools for working with document types.
/// </summary>
public interface IDocumentTypeTools
{
    /// <summary>
    /// Scan content to detect content type
    /// </summary>
    /// <param name="source">stream</param>
    /// <returns>content type</returns>
    Task<string?> DetectContentTypeAsync(Stream source);

    /// <summary>
    /// Retrieves the document type associated with the specified content type.
    /// </summary>
    /// <param name="contentType">The content type to search for.</param>
    /// <returns>The document type associated with the specified content type, or null if no matching document type is found.</returns>
    IDocumentType? GetByContentType(string contentType);

    /// <summary>
    /// Retrieves the document type associated with the specified file extension.
    /// </summary>
    /// <param name="fileExtension">The file extension to search for.</param>
    /// <returns>The document type associated with the specified file extension, or null if no matching document type is found.</returns>
    IDocumentType? GetByFileExtension(string fileExtension);

    /// <summary>
    /// Retrieves the document type associated with the content identified by the file header in the specified stream.
    /// </summary>
    /// <param name="stream">The stream containing the file header.</param>
    /// <returns>The document type associated with the content identified by the file header, or null if no matching document type is found.</returns>
    IDocumentType? GetByFileHeader(Stream stream);
}

```

```Readme.Documents.Abstractions.md
# OoBDev.Documents.Abstractions

## Overview

The OoBDev.Documents.Abstractions namespace provides interfaces and attributes for interacting with document-related 
functionalities such as blob containers, document conversion, content type detection, and document type management.

## Key Interfaces and Attributes

- **BlobContainerAttribute**: Configuration attribute for Blob Containers, specifying the container name.
- **IBlobContainerFactory**: Factory interface for building blob containers by name or type reference.
- **IBlobContainerProvider**: Interface for implementing a blob container for a particular provider type.
- **IBlobContainerProviderFactory**: Factory interface for creating a blob container by name for a specific provider type.
- **IDocumentConversionHandler**: Interface for a handler responsible for document conversion, specifying supported source and destination content types.
- **IBlobContainer**: Interface for interacting with blob containers, providing methods for content retrieval, storage, and deletion.
- **IDocumentConversion**: Interface for a service responsible for document conversion, with a method for converting documents from one format to another.
- **IDocumentTypeTools**: Interface providing tools for working with document types, including content type detection and retrieval of document types by file extension or header.
- **IContentTypeDetector**: Interface for detecting content type from a stream.
- **IDocumentType**: Interface representing a document type, providing information such as supported content types, file extensions, and file headers.

## Key Models

- **ContentMetaDataReference**: Represents a reference to content metadata, including content type, file name, and metadata.
- **ContentReference**: Represents a reference to content, containing information such as content type, file name, and content itself.
- **DocumentType**: Represents a document type, including its name, supported content types, file extensions, and file header.
- **IDocumentType**: Interface representing a document type, providing information such as name, supported content types, file extensions, and file headers.

## Getting Started

Interfaces in the OoBDev.Documents.Abstractions namespace provide contracts for implementing various document-related 
functionalities. Implement these interfaces in your application to integrate document management features such as blob 
storage, document conversion, and content type detection.

## Example Usage

```csharp
// Example of implementing IBlobContainerProvider interface
public class MyBlobContainerProvider : IBlobContainerProvider
{
    public string ContainerName { get; }

    public MyBlobContainerProvider(string containerName)
    {
        ContainerName = containerName;
    }

    public IBlobContainer CreateBlobContainer()
    {
        // Implementation to create a blob container
    }
}

// Example of using IDocumentConversion interface
public class MyDocumentConversionService : IDocumentConversion
{
    public async Task ConvertAsync(Stream source, string sourceContentType, Stream destination, string destinationContentType)
    {
        // Implementation of document conversion logic
    }
}

```

