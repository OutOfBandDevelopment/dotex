# OoBDev.SemanticKernel.Abstractions

Abstractions and interfaces for Semantic Kernel integration.

## Description

This package provides abstraction layers and dependency injection extensions for Microsoft Semantic Kernel. It defines core interfaces for chat providers and kernel plugins, enabling flexible AI integration patterns.

## Key Features

- Chat provider abstraction for LLM interactions
- Kernel plugin interface for extensibility
- Dependency injection integration
- Configuration support for Semantic Kernel services
- Type-safe kernel globals management

## Installation

```xml
<PackageReference Include="OoBDev.SemanticKernel.Abstractions" Version="*" />
```

## Basic Usage

```csharp
using OoBDev.SemanticKernel;
using Microsoft.Extensions.DependencyInjection;

services.AddSemanticKernelAbstractions(configuration);

// Implement custom chat provider
public class MyChatProvider : IChatProvider
{
    public async Task<string?> OneShotAsync(string prompt)
    {
        // Your implementation
    }
}
```

## License

See repository license file for details.

## Repository

[OoBDev Repository](https://github.com/yourusername/oobdev)
