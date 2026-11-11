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
    .UsePlantuml()
    .Build();

var html = Markdown.ToHtml(markdownText, pipeline);
```

## PlantUML server

Diagrams are rendered by a PlantUML server. `PlantUmlRendererOptions.RemoteUrl` defaults to the local `plantuml` container (`http://localhost:8086/`), so no diagram source leaves the machine unless you set a public server on purpose; a blank URL fails with a clear message.

```csharp
var pipeline = new MarkdownPipelineBuilder()
    .UsePlantuml(new PlantUmlRendererOptions { RemoteUrl = "https://www.plantuml.com/plantuml/" })
    .Build();
```

## License

See repository license file for details.

## Repository

[OoBDev Repository](https://github.com/yourusername/oobdev)
