# OoBDev.Data.Vectors.Hosting

Background service for processing SQL Server Service Broker embedding requests.

## Description

This package provides a hosted background service that processes text embedding requests from SQL Server Service Broker queues. It integrates with embedding providers to generate vector embeddings for database content asynchronously.

## Key Features

- SQL Service Broker queue reader
- Asynchronous embedding generation
- Batch processing support
- Configurable timeout and batch size
- Integration with IEmbeddingProvider
- Hosted service lifecycle management

## Installation

```xml
<PackageReference Include="OoBDev.Data.Vectors.Hosting" Version="*" />
```

## Basic Usage

```csharp
using OoBDev.Data.Vectors.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

services.AddEmbeddingSentenceTransformerQueueReader(configuration);

// Configuration in appsettings.json
{
  "EmbeddingSentenceTransformerQueueReader": {
    "MaximumReadLength": 100,
    "ReadWaitTimeout": "00:00:30"
  },
  "ConnectionStrings": {
    "EmbeddingSentenceTransformer": "Server=localhost;Database=VectorDb;..."
  }
}
```

## License

See repository license file for details.

## Repository

[OoBDev Repository](https://github.com/yourusername/oobdev)
