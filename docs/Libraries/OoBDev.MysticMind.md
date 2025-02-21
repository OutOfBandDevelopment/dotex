# OoBDev.MysticMind


## Class: MysticMind.ConverterFactory
Factory for creating converters. 

### Methods


#### Build
Builds a new converter. 


##### Return value
A new instance of the converter.



## Class: MysticMind.HtmlToMarkdownConversionHandler
Converts HTML documents to Markdown format. 

### Properties

#### Destinations
Gets the content types supported for the destination stream.
#### Sources
Gets the content types supported for the source stream.
### Methods


#### Constructor
Constructor for HtmlToMarkdownConversionHandler 


##### Parameters
* *_converter:* 




#### ConvertAsync(System.IO.Stream,System.String,System.IO.Stream,System.String)
Converts the content of a HTML document in the source stream to Markdown format and writes it to the destination stream. 


##### Parameters
* *source:* The source stream containing the HTML document.
* *sourceContentType:* The content type of the source stream.
* *destination:* The destination stream where the Markdown content will be written.
* *destinationContentType:* The content type of the destination stream.




##### Return value
A task representing the asynchronous conversion operation.



##### Exceptions

* *System.NotSupportedException:* Thrown when either the source or destination content type is not supported.




#### SupportedDestination(System.String)
Checks if the specified content type is supported for the destination stream. 


##### Parameters
* *contentType:* The content type to check.




##### Return value
True if the content type is supported; otherwise, false.



#### SupportedSource(System.String)
Checks if the specified content type is supported for the source stream. 


##### Parameters
* *contentType:* The content type to check.




##### Return value
True if the content type is supported; otherwise, false.



## Class: MysticMind.IConverterFactory
Interface for a factory that creates converters. 

### Methods


#### Build
Builds a new converter. 


##### Return value
A new instance of the converter.



## Class: MysticMind.ServiceCollectionExtensions
Provides extension methods for configuring services related to MysticMind. 

### Methods


#### TryAddMysticMindServices(Microsoft.Extensions.DependencyInjection.IServiceCollection)
Configures services for MysticMind. 


##### Parameters
* *services:* The to add the services to.




##### Return value
The modified .

