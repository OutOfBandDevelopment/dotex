# OoBDev.SBert.AllMiniLML6v2Sharp

Sentence-BERT embedding provider using the AllMiniLmL6V2 model with ONNX Runtime.

## Description

This package provides a high-performance text embedding provider using the AllMiniLmL6V2 sentence transformer model. It generates 384-dimensional vector embeddings for semantic similarity and search applications.

## Key Features

- AllMiniLmL6V2 sentence transformer model
- ONNX Runtime-based inference
- Batch embedding generation support
- Asynchronous API with streaming results
- Integration with OoBDev AI abstractions
- Configuration-based setup

## Installation

```xml
<PackageReference Include="OoBDev.SBert.AllMiniLML6v2Sharp" Version="*" />
```

## Basic Usage

```csharp
using OoBDev.SBert.AllMiniLML6v2Sharp;
using Microsoft.Extensions.DependencyInjection;

services.AddAllMiniLmL6V2Embedding(configuration);

// Generate embeddings
var embedding = await embeddingProvider.GenerateEmbeddingAsync(
    "Your text here",
    model: null,
    cancellationToken);
```

## License

See repository license file for details.

## Repository

[OoBDev Repository](https://github.com/yourusername/oobdev)
