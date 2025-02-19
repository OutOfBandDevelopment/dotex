# OoBDev.MysticMind

OoBDev.MysticMind provides functionality for converting HTML documents to Markdown format. Let's explore its features:

## ConverterFactory

This class is a factory for creating converters.

### Methods

- **Build**: Builds a new converter.

## HtmlToMarkdownConversionHandler

Converts HTML documents to Markdown format.

### Properties

- **Destinations**: Gets the content types supported for the destination stream.
- **Sources**: Gets the content types supported for the source stream.

### Methods

- **Constructor**: Constructor for HtmlToMarkdownConversionHandler.
- **ConvertAsync(source, sourceContentType, destination, destinationContentType)**: Converts the content of an HTML document in the source stream to Markdown format and writes it to the destination stream.
- **SupportedDestination(contentType)**: Checks if the specified content type is supported for the destination stream.
- **SupportedSource(contentType)**: Checks if the specified content type is supported for the source stream.

## IConverterFactory

Interface for a factory that creates converters.

### Methods

- **Build**: Builds a new converter.

## ServiceCollectionExtensions

Provides extension methods for configuring services related to MysticMind.

### Methods

- **TryAddMysticMindServices(services)**: Configures services for MysticMind.

OoBDev.MysticMind simplifies the conversion of HTML documents to Markdown format, providing flexibility and ease of use.
