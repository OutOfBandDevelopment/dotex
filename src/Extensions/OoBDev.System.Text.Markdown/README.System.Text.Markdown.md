# OoBDev.System.Text.Markdown

Markdown extensions for rendering PlantUML diagrams in HTML and GitHub-flavored markdown.

## Description

This package extends the Markdig markdown processor with support for PlantUML diagram rendering. It provides custom block parsers and renderers for seamlessly integrating PlantUML diagrams into your markdown documents.

## Key Features

- PlantUML block parsing in markdown documents
- HTML rendering with inline PlantUML diagrams
- GitHub-flavored markdown rendering support
- Integration with Markdig pipeline
- Automatic diagram generation from PlantUML syntax

## Installation

```xml
<PackageReference Include="OoBDev.System.Text.Markdown" Version="*" />
```

## Basic Usage

```csharp
using Markdig;
using OoBDev.System.Text.Markdown;

var pipeline = new MarkdownPipelineBuilder()
    .Use<PlantUmlExtension>()
    .Build();

var html = Markdown.ToHtml(markdownText, pipeline);
```

## License

See repository license file for details.

## Repository

[OoBDev Repository](https://github.com/yourusername/oobdev)
