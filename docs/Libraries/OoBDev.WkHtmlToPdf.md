# OoBDev.WkHtmlToPdf


## Class: WkHtmlToPdf.HtmlToPdfConversionHandler
Handler for converting HTML content to PDF format. 

### Properties

#### Destinations
Gets the supported content types for destination PDF files.
#### Sources
Gets the supported content types for source HTML files.
### Methods


#### Constructor
Initializes a new instance of the 
 *See: T:OoBDev.WkHtmlToPdf.HtmlToPdfConversionHandler*class. 


##### Parameters
* *converter:* The converter used for HTML to PDF conversion.




#### ConvertAsync(System.IO.Stream,System.String,System.IO.Stream,System.String)
Converts HTML content from a source stream to PDF format and writes it to a destination stream. 


##### Parameters
* *source:* The source stream containing the HTML content.
* *sourceContentType:* The content type of the source.
* *destination:* The destination stream to write the PDF content.
* *destinationContentType:* The content type of the destination.




##### Return value
A task representing the asynchronous operation.



##### Exceptions

* *System.NotSupportedException:* Thrown if the source or destination content type is not supported.




#### SupportedDestination(System.String)
Checks if the specified content type is supported for PDF conversion. 


##### Parameters
* *contentType:* The content type to check.




##### Return value
true if the content type is supported; otherwise, false.



#### SupportedSource(System.String)
Checks if the specified content type is supported as source HTML for PDF conversion. 


##### Parameters
* *contentType:* The content type to check.




##### Return value
true if the content type is supported; otherwise, false.



## Class: WkHtmlToPdf.ServiceCollectionExtensions
Provides extension methods for configuring services related to WkHtmlToPdf. 

### Methods


#### TryAddWkHtmlToPdfServices(Microsoft.Extensions.DependencyInjection.IServiceCollection)
Configures services for WkHtmlToPdf. 


##### Parameters
* *services:* The to add the services to.




##### Return value
The modified .

