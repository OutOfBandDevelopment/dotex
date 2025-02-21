# OoBDev.Apache.Tika


## Class: Apache.Tika.ApacheTikaClientOptions
Represents options for configuring the Apache Tika client. 

### Properties

#### Url
Gets or sets the URL of the Apache Tika server.

## Class: Apache.Tika.Detectors.TikaContentTypeDetector
Provides functionality to detect the content type of a stream using Apache Tika. 

### Methods


#### DetectContentTypeAsync(System.IO.Stream)
Asynchronously detects the content type of the provided stream using Apache Tika. 


##### Parameters
* *source:* The stream whose content type needs to be detected.




##### Return value
A representing the asynchronous operation. The task result contains the detected content type as a string, or null if the content type cannot be determined.



## Class: Apache.Tika.Handlers.TikaConversionHandlerBase
Provides a base class for document conversion handlers using Apache Tika. 

### Properties

#### Destinations
Gets an array of supported destination content types for conversion.
#### Sources
Gets an array of supported source content types for conversion.
### Methods


#### ConvertAsync(System.IO.Stream,System.String,System.IO.Stream,System.String)
Asynchronously converts a document from the source stream to the destination stream. 


##### Parameters
* *source:* The stream containing the source document.
* *sourceContentType:* The content type of the source document.
* *destination:* The stream to which the converted document will be written.
* *destinationContentType:* The desired content type of the converted document.




##### Exceptions

* *System.NotSupportedException:* Thrown when either the source content type or the destination content type is not supported.




#### SupportedDestination(System.String)
Checks if the specified content type is supported for conversion to a destination format. 


##### Parameters
* *contentType:* The content type to check.




##### Return value
true if the content type is supported; otherwise, false.



#### SupportedSource(System.String)
Checks if the specified content type is supported as a source format for conversion. 


##### Parameters
* *contentType:* The content type to check.




##### Return value
true if the content type is supported; otherwise, false.



## Class: Apache.Tika.Handlers.TikaDocToHtmlConversionHandler
Provides functionality to convert Microsoft Word documents to HTML using Apache Tika. 

### Properties

#### Sources
Gets an array of supported source content types for conversion.
### Methods


#### Constructor
Constructor to convert Microsoft Word documents to HTML using Apache Tika. 


##### Parameters
* *client:* client interface
* *logger:* system logger




## Class: Apache.Tika.Handlers.TikaDocxToHtmlConversionHandler
Provides functionality to convert Microsoft Word (DOCX) documents to HTML using Apache Tika. 

### Properties

#### Sources
Gets an array of supported source content types for conversion.
### Methods


#### Constructor
Constructor to convert Microsoft Word (DOCX) documents to HTML using Apache Tika. 


##### Parameters
* *client:* client interface
* *logger:* system logger




## Class: Apache.Tika.Handlers.TikaEpubToHtmlConversionHandler
Represents a handler for converting EPUB files to HTML using Apache Tika. 

### Properties

#### Sources
Gets an array of supported source content types for conversion.
### Methods


#### Constructor
Initializes a new instance of the 
 *See: T:OoBDev.Apache.Tika.Handlers.TikaEpubToHtmlConversionHandler*class. 


##### Parameters
* *client:* The Apache Tika client used for conversion.
* *logger:* The logger for logging conversion activities.




## Class: Apache.Tika.Handlers.TikaOdtToHtmlConversionHandler
Provides functionality to convert OpenDocument Text (ODT) documents to HTML using Apache Tika. 

### Properties

#### Sources
Gets an array of supported source content types for conversion.
### Methods


#### Constructor
Constructor to convert OpenDocument Text (ODT) documents to HTML using Apache Tika. 


##### Parameters
* *client:* client interface
* *logger:* system logger




## Class: Apache.Tika.Handlers.TikaPdfToHtmlConversionHandler
Provides functionality to convert PDF documents to HTML using Apache Tika. 

### Properties

#### Sources
Gets an array of supported source content types for conversion.
### Methods


#### Constructor
Constructor to convert Adobe PDF documents to HTML using Apache Tika. 


##### Parameters
* *client:* client interface
* *logger:* system logger




## Class: Apache.Tika.Handlers.TikaRtfToHtmlConversionHandler
Provides functionality to convert Rich Text Format (RTF) documents to HTML using Apache Tika. 

### Properties

#### Sources
Gets an array of supported source content types for conversion.
### Methods


#### Constructor
Constructor to convert Rich Text Format (RTF) documents to HTML using Apache Tika. 


##### Parameters
* *client:* client interface
* *logger:* system logger




## Class: Apache.Tika.Handlers.TikaToHtmlConversionBaseHandler
Provides a base class for handlers that convert documents to HTML using Apache Tika. 

### Properties

#### Destinations
Gets an array of supported destination content types for conversion.
### Methods


#### Constructor
Constructor to convert handler documents to HTML using Apache Tika. 


##### Parameters
* *client:* client interface
* *logger:* system logger




## Class: Apache.Tika.ServiceCollectionExtensions
Provides extension methods for configuring services related to Apache - Tika. 

### Methods


#### TryAddApacheTikaServices(Microsoft.Extensions.DependencyInjection.IServiceCollection,Microsoft.Extensions.Configuration.IConfiguration,System.String)
Configures services for Apache - Tika. 


##### Parameters
* *services:* The to add the services to.




##### Return value
The modified .

