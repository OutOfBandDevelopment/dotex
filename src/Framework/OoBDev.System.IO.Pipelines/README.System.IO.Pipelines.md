# OoBDev.System.IO.Pipelines

High-performance pipeline builder for stream processing with segmentation support.

## Description

This package provides a fluent API for building high-performance data processing pipelines using System.IO.Pipelines. It enables efficient stream processing with custom segmentation and error handling capabilities.

## Key Features

- Fluent pipeline builder API
- Stream-based pipeline creation
- Custom segmenter support for data partitioning
- Configurable error handling for readers and writers
- Asynchronous pipeline execution
- Integration with System.IO.Pipelines

## Installation

```xml
<PackageReference Include="OoBDev.System.IO.Pipelines" Version="*" />
```

## Basic Usage

```csharp
using OoBDev.System.IO.Pipelines;

var inputStream = File.OpenRead("input.txt");
var outputStream = File.OpenWrite("output.txt");

await inputStream
    .Follow()
    .With(segmenter)
    .OnError(ex => Console.WriteLine($"Error: {ex.Message}"))
    .RunAsync(cancellationToken);
```

## License

See repository license file for details.

## Repository

[OoBDev Repository](https://github.com/yourusername/oobdev)
