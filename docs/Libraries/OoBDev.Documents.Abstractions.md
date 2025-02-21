# OoBDev.Documents.Abstractions


## Class: Documents.BlobContainerAttribute
Configuration attribute for Blob Containers 

### Properties

#### ContainerName
Blob Container Name

## Class: Documents.Containers.IBlobContainerFactory
this factory is used to build blob containers by name or type reference 

### Methods


#### Create(System.String)
This is used to create a blob container with the provided name 


##### Parameters
* *containerName:* name of the collection within the blob store




##### Return value




#### Create``1
This is used to create a blob container by resolving the container name based on the referenced type 


##### Return value




## Class: Documents.Containers.IBlobContainerProvider
Implementation of a blob container for a particular provider type 

### Properties

#### ContainerName
Name of the related container
### Methods


#### 
factory method to create a blob container by related name for a particular provider type 


##### Parameters
* *containerName:* name of container




##### Return value
instance of the blob provider



## Class: Documents.Containers.IBlobContainerProviderFactory
Implementation factory for a blob store 

### Methods


#### Create(System.String)
factory method to create a blob container by related name for a particular provider type 


##### Parameters
* *containerName:* name of container




##### Return value
instance of the blob provider



## Class: Documents.Conversion.IDocumentConversionHandler
Represents a handler for document conversion. 

### Properties

#### Sources
Gets the supported source content types for document conversion.
#### Destinations
Gets the supported destination content types for document conversion.
### Methods


#### ConvertAsync(System.IO.Stream,System.String,System.IO.Stream,System.String)
Converts a document from one format to another. 


##### Parameters
* *source:* The source stream containing the document to convert.
* *sourceContentType:* The content type of the source document.
* *destination:* The destination stream where the converted document will be written.
* *destinationContentType:* The desired content type of the converted document.




##### Return value
A task representing the asynchronous operation.



#### SupportedSource(System.String)
Determines whether the specified content type is supported as a source for document conversion. 


##### Parameters
* *contentType:* The content type to check.




##### Return value
True if the content type is supported; otherwise, false.



#### SupportedDestination(System.String)
Determines whether the specified content type is supported as a destination for document conversion. 


##### Parameters
* *contentType:* The content type to check.




##### Return value
True if the content type is supported; otherwise, false.



## Class: Documents.IBlobContainer
Interface for interacting with blob containers. 

### Methods


#### GetContentAsync(System.String)
Gets the content reference for the specified path. 


##### Parameters
* *path:* The path to the content.




##### Return value
The content reference, or null if not found.



#### GetContentMetaDataAsync(System.String)
Gets the content metadata reference for the specified path. 


##### Parameters
* *path:* The path to the content.




##### Return value
The content metadata reference, or null if not found.



#### StoreContentAsync(OoBDev.Documents.Models.ContentReference,System.Collections.Generic.Dictionary{System.String,System.String},System.Boolean)
Stores the content with the specified reference, metadata, and overwrite option. 


##### Parameters
* *reference:* The content reference.
* *metadata:* The metadata to store.
* *overwrite:* Whether to overwrite the existing content.




##### Return value
A task representing the asynchronous operation.



#### StoreContentMetaDataAsync(OoBDev.Documents.Models.ContentMetaDataReference)
Stores the content metadata with the specified reference. 


##### Parameters
* *reference:* The content metadata reference.




##### Return value
A task representing the asynchronous operation.



#### QueryContent
Queries the content metadata. 


##### Return value
A queryable collection of content metadata references.



#### DeleteContentAsync(System.String)
Deletes the content at the specified path. 


##### Parameters
* *path:* The path to the content.




##### Return value
A task representing the asynchronous operation.



## Class: Documents.IBlobContainer`1
Interface for interacting with blob containers. 


## Class: Documents.IContentTypeDetector
Represents a contract for detecting content type from a stream. 

### Methods


#### DetectContentTypeAsync(System.IO.Stream)
Asynchronously detects the content type of the provided stream. 


##### Parameters
* *source:* The stream to detect the content type from.




##### Return value
A task representing the asynchronous operation. The task result contains the detected content type as a string, or null if the content type cannot be determined.



## Class: Documents.IDocumentConversion
Represents a service for document conversion. 

### Methods


#### ConvertAsync(System.IO.Stream,System.String,System.IO.Stream,System.String)
Converts a document from one format to another. 


##### Parameters
* *source:* The source stream containing the document to convert.
* *sourceContentType:* The content type of the source document.
* *destination:* The destination stream where the converted document will be written.
* *destinationContentType:* The desired content type of the converted document.




##### Return value
A task representing the asynchronous operation.



## Class: Documents.IDocumentTypeTools
Provides tools for working with document types. 

### Methods


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
The document type associated with the specified content type, or null if no matching document type is found.



#### GetByFileExtension(System.String)
Retrieves the document type associated with the specified file extension. 


##### Parameters
* *fileExtension:* The file extension to search for.




##### Return value
The document type associated with the specified file extension, or null if no matching document type is found.



#### GetByFileHeader(System.IO.Stream)
Retrieves the document type associated with the content identified by the file header in the specified stream. 


##### Parameters
* *stream:* The stream containing the file header.




##### Return value
The document type associated with the content identified by the file header, or null if no matching document type is found.



## Class: Documents.Models.ContentMetaDataReference
Represents a reference to content metadata. 

### Properties

#### ContentType
Gets or initializes the content type of the content.
#### FileName
Gets or initializes the file name associated with the content.
#### MetaData
Gets or initializes the metadata associated with the content.

## Class: Documents.Models.ContentReference
Represents a content reference containing information about content such as a stream, content type, and file name. 

### Properties

#### Content
Gets or initializes the content type of the content.
#### ContentType
Gets or initializes the content type of the content.
#### FileName
Gets or initializes the file name associated with the content.

## Class: Documents.Models.DocumentType
Represents a document type, including its name, supported content types, file extensions, and file header. 

### Properties

#### Name
Gets or sets the name of the document type.
#### ContentTypes
Gets or sets the MIME content types supported by this document type.
#### FileExtensions
Gets or sets the file extensions commonly associated with this document type.
#### FileHeader
Gets or sets the unique byte sequence at the beginning of files of this type.

## Class: Documents.Models.IDocumentType
Represents a document type. 

### Properties

#### Name
Gets the name of the document type.
#### ContentTypes
Gets the supported content types for documents of this type.
#### FileExtensions
Gets the supported file extensions for documents of this type.
#### FileHeader
Gets the file header bytes that identify documents of this type.