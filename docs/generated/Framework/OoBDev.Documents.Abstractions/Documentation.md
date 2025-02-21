Here is the documentation for the provided source code in Markdown format, including PlantUML diagrams.

**Overview**
==========

This documentation provides an overview of the OoBDev.Documents namespace, which contains interfaces and attributes for working with blob containers and document conversion.

**Source Files**
--------------

### `BlobContainerAttribute.cs`

```cs
using System;

namespace OoBDev.Documents
{
    /// <summary>
    /// Configuration attribute for Blob Containers
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class BlobContainerAttribute : Attribute
    {
        /// <summary>
        /// Blob Container Name
        /// </summary>
        public string? ContainerName { get; set; }
    }
}
```

### `OoBDev.Documents.Abstractions.csproj`

```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <ImplicitUsings>false</ImplicitUsings>
    <Nullable>enable</Nullable>
    <GenerateDocumentationFile>True</GenerateDocumentationFile>
    <RootNamespace>$(MSBuildProjectName.Replace(" ", "_").Replace(".Abstractions", ""))</RootNamespace>
    <GenerateAssemblyInfo>true</GenerateAssemblyInfo>
    <PackageReadmeFile>Readme.Documents.Abstractions.md</PackageReadmeFile>
  </PropertyGroup>
  <ItemGroup>
    <InternalsVisibleTo Include="OoBDev.Documents" />
    <InternalsVisibleTo Include="OoBDev.Documents.Tests" />
  </ItemGroup>
</Project>
```

### `IBlobContainer.cs`

```cs
using OoBDev.Documents.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OoBDev.Documents
{
    /// <summary>
    /// Interface for interacting with blob containers.
    /// </summary>
    public interface IBlobContainer
    {
        /// <summary>
        /// Gets the content reference for the specified path.
        /// </summary>
        /// <param name="path">The path to the content.</param>
        /// <returns>The content reference, or null if not found.</returns>
        Task<ContentReference?> GetContentAsync(string path);

        /// <summary>
        /// Gets the content metadata reference for the specified path.
        /// </summary>
        /// <param name="path">The path to the content.</param>
        /// <returns>The content metadata reference, or null if not found.</returns>
        Task<ContentMetaDataReference?> GetContentMetaDataAsync(string path);

        /// <summary>
        /// Stores the content with the specified reference, metadata, and overwrite option.
        /// </summary>
        /// <param name="reference">The content reference.</param>
        /// <param name="metadata">The metadata to store.</param>
        /// <param name="overwrite">Whether to overwrite the existing content.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task StoreContentAsync(ContentReference reference, Dictionary<string, string>? metadata = null, bool overwrite = false);

        /// <summary>
        /// Stores the content metadata with the specified reference.
        /// </summary>
        /// <param name="reference">The content metadata reference.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task<bool> StoreContentMetaDataAsync(ContentMetaDataReference reference);

        /// <summary>
        /// Queries the content metadata.
        /// </summary>
        /// <returns%A queryable collection of content metadata references.</returns>
        IQueryable<ContentMetaDataReference> QueryContent();

        /// <summary>
        /// Deletes the content at the specified path.
        /// </summary>
        /// <param name="path">The path to the content.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task DeleteContentAsync(string path);
    }

    /// <summary>
    /// Interface for interacting with blob containers.
    /// </summary>
    public interface IBlobContainer<T> : IBlobContainer
    {
    }
}
```

### `IContentTypeDetector.cs`

```cs
using System.IO;
using System.Threading.Tasks;

namespace OoBDev.Documents
{
    /// <summary>
    /// Represents a contract for detecting content type from a stream.
    /// </summary>
    public interface IContentTypeDetector
    {
        /// <summary>
        /// Asynchronously detects the content type of the provided stream.
        /// </summary>
        /// <param name="source">The stream to detect the content type from.</param>
        /// <returns>A task representing the asynchronous operation. The task result contains the detected content type as a string, or <c>null</c> if the content type cannot be determined.</returns>
        Task<string?> DetectContentTypeAsync(Stream source);
    }
}
```

### `IDocumentConversion.cs`

```cs
using System.IO;
using System.Threading.Tasks;

namespace OoBDev.Documents
{
    /// <summary>
    /// Represents a service for document