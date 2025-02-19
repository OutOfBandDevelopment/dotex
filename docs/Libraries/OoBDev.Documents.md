# OoBDev.Documents


## Class: Documents.Containers.BlobContainerFactory
Represents a factory for creating blob containers. 

### Methods


#### Constructor
Initializes a new instance of the 
 *See: T:OoBDev.Documents.Containers.BlobContainerFactory*class. 


##### Parameters
* *serviceProvider:* The IOC Service provider.
* *factories:* The factory used to create blob container providers.
* *logger:* The logger for this service.




#### Create(System.String)
Creates a blob container with the specified container name. 


##### Parameters
* *containerName:* The name of the container.




##### Return value
An instance of .



#### Create``1
Creates a blob container with a name derived from the specified type. 


##### Return value
An instance of .



## Class: Documents.Containers.WrappedBlobContainer`1
Represents a wrapper for a blob container. 

### Methods


#### Constructor
Initializes a new instance of the 
 *See: T:OoBDev.Documents.IBlobContainer*class. 


##### Parameters
* *factory:* The blob container factory to wrap.




#### DeleteContentAsync(System.String)
Deletes content asynchronously. 


##### Parameters
* *path:* The path to the content to be deleted.




##### Return value
A task representing the asynchronous operation.



#### GetContentAsync(System.String)
Retrieves content asynchronously. 


##### Parameters
* *path:* The path to the content to be retrieved.




##### Return value
A task representing the asynchronous operation. The task result contains the reference to the content.



#### GetContentMetaDataAsync(System.String)
Retrieves content metadata asynchronously. 


##### Parameters
* *path:* The path to the content.




##### Return value
A task representing the asynchronous operation. The task result contains the metadata reference.



#### QueryContent
Queries content metadata. 


##### Return value
An IQueryable representing the content metadata.



#### StoreContentAsync(OoBDev.Documents.Models.ContentReference,System.Collections.Generic.Dictionary{System.String,System.String},System.Boolean)
Stores content asynchronously. 


##### Parameters
* *reference:* The reference to the content.
* *metadata:* The metadata associated with the content.
* *overwrite:* Determines whether to overwrite existing content with the same name.




##### Return value
A task representing the asynchronous operation.



#### StoreContentMetaDataAsync(OoBDev.Documents.Models.ContentMetaDataReference)
Stores content metadata asynchronously. 


##### Parameters
* *reference:* The reference to the content metadata.




##### Return value
A task representing the asynchronous operation. The task result indicates whether the operation was successful.



## Class: Documents.Conversion.ChainStep
Represents a step in a document conversion chain. 

### Properties

#### Handler
Gets or sets the document conversion handler for this step.
#### SourceContentType
Gets or sets the content type of the source document for this step.
#### DestinationContentType
Gets or sets the content type of the destination document for this step.

## Class: Documents.Conversion.DocumentConversion
Represents a document conversion service. 

### Methods


#### Constructor
Initializes a new instance of the 
 *See: T:OoBDev.Documents.Conversion.DocumentConversion*class with the specified document conversion chain builder. 


##### Parameters
* *chain:* The document conversion chain builder.
* *tools:* The document type tools.
* *logger:* The logger.




#### ConvertAsync(System.IO.Stream,System.String,System.IO.Stream,System.String)
Converts the content of a source stream to a destination stream asynchronously. 


##### Parameters
* *source:* The source stream containing the content to be converted.
* *sourceContentType:* The content type of the source content.
* *destination:* The destination stream where the converted content will be written.
* *destinationContentType:* The content type of the converted content.




##### Return value
A representing the asynchronous operation.



#### Constructor
Initializes a new instance of the 
 *See: T:OoBDev.Documents.Conversion.DocumentConversionChainBuilder*class. 


##### Parameters
* *handlers:* The available document conversion handlers.




#### 
Constructs the document conversion steps for converting from the specified source content type to the destination content type. 


##### Parameters
* *sourceContentType:* The content type of the source document.
* *destinationContentType:* The desired content type of the converted document.




##### Return value
An array of ChainStep objects representing the conversion steps.



## Class: Documents.Conversion.DocumentConversionChainBuilder
Represents a builder for constructing document conversion chains. 

### Methods


#### Constructor
Initializes a new instance of the 
 *See: T:OoBDev.Documents.Conversion.DocumentConversionChainBuilder*class. 


##### Parameters
* *handlers:* The available document conversion handlers.




#### Steps(System.String,System.String)
Constructs the document conversion steps for converting from the specified source content type to the destination content type. 


##### Parameters
* *sourceContentType:* The content type of the source document.
* *destinationContentType:* The desired content type of the converted document.




##### Return value
An array of ChainStep objects representing the conversion steps.



## Class: Documents.Conversion.IDocumentConversionChainBuilder
Represents a document conversion chain builder. 

### Methods


#### Steps(System.String,System.String)
Gets the conversion steps from a source content type to a destination content type. 


##### Parameters
* *sourceContentType:* The content type of the source content.
* *destinationContentType:* The content type of the destination content.




##### Return value
An array of representing the conversion steps.



## Class: Documents.DocumentTypeTools
Represents a toolset for managing document types. 

### Methods


#### Constructor
Initializes a new instance of the 
 *See: T:OoBDev.Documents.DocumentTypeTools*class with the specified document types. 


##### Parameters
* *types:* The collection of document types.
* *contentTypeDetector:* The content type detector.




#### DetectContentTypeAsync(System.IO.Stream)
Scan content to detect content type 


##### Parameters
* *source:* stream




##### Return value
content type



#### GetByContentType(System.String)
Retrieves the document type associated with the specified content type. 


##### Parameters
* *contentType:* The content type to search for.




##### Return value
The document type associated with the content type, if found; otherwise, null.



#### GetByFileExtension(System.String)
Retrieves the document type associated with the specified file extension. 


##### Parameters
* *fileExtension:* The file extension to search for.




##### Return value
The document type associated with the file extension, if found; otherwise, null.



#### GetByFileHeader(System.IO.Stream)
Retrieves the document type associated with the specified file header. 


##### Parameters
* *stream:* The stream containing the file header.




##### Return value
The document type associated with the file header, if found; otherwise, null.



## Class: Documents.ServiceCollectionExtensions
Provides extension methods for configuring document-related services. 

### Methods


#### TryAddDocumentServices(Microsoft.Extensions.DependencyInjection.IServiceCollection)
Configures services for document processing. 


##### Parameters
* *services:* The to add the services to.




##### Return value
The modified .

