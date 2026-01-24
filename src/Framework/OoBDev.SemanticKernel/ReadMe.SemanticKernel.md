# OoBDev.SemanticKernel

Core Semantic Kernel implementations with built-in plugins.

## Description

This package provides concrete implementations and built-in plugins for Microsoft Semantic Kernel, including time-based functions and current user context plugins.

## Key Features

- Built-in time plugin for temporal functions
- Current user plugin for user context
- Dependency injection integration
- Ready-to-use kernel plugin implementations
- Configuration-based setup

## Installation

```xml
<PackageReference Include="OoBDev.SemanticKernel" Version="*" />
```

## Basic Usage

```csharp
using OoBDev.SemanticKernel;
using Microsoft.Extensions.DependencyInjection;

services.AddSemanticKernel(configuration);

// Plugins are automatically registered and available in the kernel
```

## License

See repository license file for details.

## Repository

[OoBDev Repository](https://github.com/yourusername/oobdev)
