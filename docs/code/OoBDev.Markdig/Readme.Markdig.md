# OoBDev.Markdig

## MarkdownToHtmlConversionHandler

Converts Markdown documents to HTML format.

### Properties

- **Destinations**: Gets the supported content types for the destination stream.
- **Sources**: Gets the supported content types for the source stream.

### Methods

#### Constructor

Initializes a new instance of MarkdownToHtmlConversionHandler.

##### Parameters

- *pipeline*: (optional) The pipeline to use for conversion.

#### ConvertAsync(System.IO.Stream,System.String,System.IO.Stream,System.String)

Converts Markdown content from the source stream to HTML format and writes it to the destination stream asynchronously.

##### Parameters

- *source*: The source stream containing the Markdown content.
- *sourceContentType*: The content type of the source stream.
- *destination*: The destination stream where the HTML content will be written.
- *destinationContentType*: The content type of the destination stream.

##### Return value

A task representing the asynchronous conversion operation.

##### Exceptions

- *System.NotSupportedException*: Thrown when either the source or destination content type is not supported.

#### SupportedDestination(System.String)

Checks if the specified content type is supported for the destination stream.

##### Parameters

- *contentType*: The content type to check.

##### Return value

True if the content type is supported; otherwise, false.

#### SupportedSource(System.String)

Checks if the specified content type is supported for the source stream.

##### Parameters

- *contentType*: The content type to check.

##### Return value

True if the content type is supported; otherwise, false.

## ServiceCollectionExtensions

Provides extension methods for configuring services related to Markdig.

### Methods

#### TryAddMarkdigServices(Microsoft.Extensions.DependencyInjection.IServiceCollection)

Configures services for Markdig.

##### Parameters

- *services*: The service collection to add the services to.

##### Return value

The modified service collection.
