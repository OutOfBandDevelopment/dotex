# OoBDev.System


## Class: System.IO.TempFileFactory
Represents a factory for creating temporary files. 

### Methods


#### Constructor
Initializes a new instance of the 
 *See: T:OoBDev.System.IO.TempFileFactory*class. 


##### Parameters
* *serviceProvider:* The service provider used for creating instances.




#### GetTempFile
Creates a new temporary file. 


##### Return value
A temporary file.



## Class: System.IO.TempFileHandle
Represents a handle for managing temporary files. 

### Properties

#### FilePath
Gets the path of the temporary file.
### Methods


#### Constructor
Initializes a new instance of the 
 *See: T:OoBDev.System.IO.TempFileHandle*class. 


##### Parameters
* *logger:* The logger used for logging operations.




#### ToString
Returns the path of the temporary file. 


##### Return value
The path of the temporary file.



#### Finalize
Releases the resources used by the temporary file. 


#### Dispose
Releases the resources used by the temporary file. 


## Class: System.Providers.DateTimeProvider
Provides date and time functionality. 

### Properties

#### Now
Gets the current local date and time. This property returns the current local date and time.
#### UtcNow
Gets the current Coordinated Universal Time (UTC) date and time. This property returns the current Coordinated Universal Time (UTC) date and time.

## Class: System.Providers.GuidProvider
Represents a provider for generating and handling GUIDs. 

### Properties

#### Empty
Gets an empty GUID. This property returns a GUID with all bits set to zero.
### Methods


#### NewGuid
Generates a new GUID. 


##### Return value
A new GUID.



## Class: System.Security.Cryptography.HashTypes
Specifies different types of hash algorithms. 

### Fields

#### Md5
Represents the MD5 hash algorithm.
#### Sha256
Represents the SHA-256 hash algorithm.
#### Sha512
Represents the SHA-512 hash algorithm.

## Class: System.Security.Cryptography.Md5Hash
Default hash of input value. Base64 encoded MD5 Hash 

### Methods


#### GetHash(System.String)
Computes the default hash of the input value using MD5. 


##### Parameters
* *value:* The input value to be hashed.




##### Return value
The Base64 encoded MD5 hash of the input value.



## Class: System.Security.Cryptography.Sha256Hash
Default hash of input value. Base64 encoded SHA256 Hash 

### Methods


#### GetHash(System.String)
Computes the default hash of the input value using SHA256. 


##### Parameters
* *value:* The input value to be hashed.




##### Return value
The Base64 encoded SHA256 hash of the input value.



## Class: System.Security.Cryptography.Sha512Hash
Default hash of input value. Base64 encoded SHA512 Hash 

### Methods


#### GetHash(System.String)
Computes the default hash of the input value using SHA512. 


##### Parameters
* *value:* The input value to be hashed.




##### Return value
The Base64 encoded SHA512 hash of the input value.



## Class: System.ServiceCollectionExtensions
Suggested IOC configurations 

### Methods


#### TryAddSystemExtensions(Microsoft.Extensions.DependencyInjection.IServiceCollection,Microsoft.Extensions.Configuration.IConfiguration,OoBDev.System.SystemExtensionBuilder)
This will add all available extensions to the IOC container 


##### Parameters
* *services:* 
* *config:* 
* *builder:* 




##### Return value




#### TrySecurityExtensions(Microsoft.Extensions.DependencyInjection.IServiceCollection,OoBDev.System.Security.Cryptography.HashTypes)
Add support for shared security extensions 


##### Parameters
* *services:* 
* *defaultHashType:* 




##### Return value




#### TrySerializerExtensions(Microsoft.Extensions.DependencyInjection.IServiceCollection,OoBDev.System.Text.SerializerTypes)
Add support for shared Serializer 


##### Parameters
* *services:* 
* *defaultSerializerType:* 




##### Return value




#### TryTemplatingExtensions(Microsoft.Extensions.DependencyInjection.IServiceCollection,Microsoft.Extensions.Configuration.IConfiguration,System.String)
Add support for shared Templating 


##### Parameters
* *services:* 
* *configuration:* The to add services to.
* *configurationSection:* 




##### Return value




#### TryAddProviders(Microsoft.Extensions.DependencyInjection.IServiceCollection)
Add support for type providers 


##### Parameters
* *services:* 




##### Return value




## Class: System.SystemExtensionBuilder
Represents a builder for configuring system extensions. 

### Properties

#### FileTemplatingConfigurationSection
Gets or sets the configuration section name for file templating options. The configuration section name for file templating options. Default is "FileTemplatingOptions".
#### DefaultHashType
Gets or sets the default hash type to be used. The default hash type. The default value is .
#### DefaultSerializerType
Gets or sets the default serializer type to be used. The default serializer type. The default value is .
#### StringCasingOrder
Get or sets the default string casing order

## Class: System.Text.Json.BsonDateTimeOffsetConverter
System.Text.Json converter to support BsonDatetimeOffset 

### Methods


#### CanConvert(System.Type)
Determines whether this converter can convert the specified type. 


##### Parameters
* *typeToConvert:* The type to check for conversion support.




##### Return value
true if the converter can convert the specified type; otherwise, false.



#### Read(System.Text.Json.Utf8JsonReader@,System.Type,System.Text.Json.JsonSerializerOptions)
Reads the JSON representation of the object. 


##### Parameters
* *reader:* The reader to read from.
* *typeToConvert:* The type of the object to convert.
* *options:* The serializer options to use during conversion.




##### Return value
The deserialized object value.



#### Write(System.Text.Json.Utf8JsonWriter,System.Object,System.Text.Json.JsonSerializerOptions)
Writes the JSON representation of the object. 


##### Parameters
* *writer:* The writer to write to.
* *value:* The value to convert.
* *options:* The serializer options to use during conversion.




## Class: System.Text.Json.BsonIdConverter
This type converter for System.Text.Json to support BSON ObjectID to JSON safe export/import 

### Methods


#### Read(System.Text.Json.Utf8JsonReader@,System.Type,System.Text.Json.JsonSerializerOptions)
read value from Bson and if object id is bson object id convert it to string 


##### Parameters
* *reader:* 
* *typeToConvert:* 
* *options:* 




##### Return value




##### Exceptions

* *System.NotSupportedException:* 




#### Write(System.Text.Json.Utf8JsonWriter,System.String,System.Text.Json.JsonSerializerOptions)
write object id to Bson object 


##### Parameters
* *writer:* 
* *value:* 
* *options:* 




## Class: System.Text.Json.BsonTypeInfoResolver
Resolves JSON type information for BSON serialization. 

### Methods


#### GetTypeInfo(System.Type,System.Text.Json.JsonSerializerOptions)
Gets the JSON type information for the specified type during BSON serialization. 


##### Parameters
* *type:* The type for which to get JSON type information.
* *options:* The used for serialization.




##### Return value
The JSON type information for the specified type.



## Class: System.Text.Json.ConfigurationJsonConverter`1
JsonConverter for serializing and deserializing IConfiguration instances. 
The type of IConfiguration. 

### Methods


#### Read(System.Text.Json.Utf8JsonReader@,System.Type,System.Text.Json.JsonSerializerOptions)
Reads the JSON representation of an IConfiguration instance and converts it. 


##### Parameters
* *reader:* The reader to read from.
* *typeToConvert:* The type to convert.
* *options:* The serializer options.




##### Return value
The converted IConfiguration instance.



#### Write(System.Text.Json.Utf8JsonWriter,`0,System.Text.Json.JsonSerializerOptions)
Writes the JSON representation of an IConfiguration instance. 


##### Parameters
* *writer:* The writer to write to.
* *value:* The IConfiguration instance to write.
* *options:* The serializer options.




## Class: System.Text.Json.DictionaryStringObjectJsonConverter
Custom JSON converter for dictionaries with string keys and object values. 

### Methods


#### CanConvert(System.Type)
Determines whether this converter can convert the specified type. 


##### Parameters
* *typeToConvert:* The type to convert.




##### Return value
true if the converter can convert the specified type; otherwise, false.



#### Read(System.Text.Json.Utf8JsonReader@,System.Type,System.Text.Json.JsonSerializerOptions)
Reads the JSON representation of a 
 *See: T:System.Collections.Generic.Dictionary`2*with string keys and object values. 


##### Parameters
* *reader:* The reader to read from.
* *typeToConvert:* The type to convert.
* *options:* The serializer options.




##### Return value
The converted dictionary.



#### Write(System.Text.Json.Utf8JsonWriter,System.Collections.Generic.Dictionary{System.String,System.Object},System.Text.Json.JsonSerializerOptions)
Writes the JSON representation of a 
 *See: T:System.Collections.Generic.Dictionary`2*with string keys and object values. 


##### Parameters
* *writer:* The writer to write to.
* *value:* The dictionary to write.
* *options:* The serializer options.




## Class: System.Text.Json.JsonNodeExtensions
shared extension methods for System.Text.Json 

### Methods


#### ToXFragment(System.Text.Json.Nodes.JsonObject,System.String)
Converts a JsonObject to an XFragment. 


##### Parameters
* *json:* The JsonObject to convert.
* *rootName:* The root name for the resulting XML element.




##### Return value
An XFragment representing the JsonObject.



#### ToXFragment(System.Text.Json.Nodes.JsonArray,System.String)
Converts a JsonArray to an XFragment. 


##### Parameters
* *json:* The JsonArray to convert.
* *rootName:* The root name for the resulting XML element.




##### Return value
An XFragment representing the JsonArray.



#### ToXFragment(System.Text.Json.Nodes.JsonNode,System.String)
Converts a JsonNode to an XFragment. 


##### Parameters
* *json:* The JsonNode to convert.
* *rootName:* The root name for the resulting XML element.




##### Return value
An XFragment representing the JsonNode.



## Class: System.Text.Json.Serialization.DefaultBsonSerializer
Default serializer for BSON (Binary JSON). 

### Fields

#### DefaultContentType
The default content type for BSON.
### Properties

#### ContentType
Gets the content type for BSON, which is "application/json".
### Methods


#### Constructor
Initializes a new instance of the 
 *See: T:OoBDev.System.Text.Json.Serialization.DefaultBsonSerializer*class. 


## Class: System.Text.Json.Serialization.DefaultJsonSerializer
Default serializer for JSON. 

Initializes a new instance of the class.
### Fields

#### DefaultContentType
Gets the default content type for JSON.
#### _options
The JSON serializer options.
### Properties

#### DefaultOptions
Gets the default JSON serializer options.
#### ContentType
Gets the content type for JSON.
### Methods


#### Constructor
Initializes a new instance of the class.
Default serializer for JSON. 


##### Parameters
* *options:* Optional JSON serializer options.




#### Serialize``1(``0)
Serializes an object to a JSON string. 


#### Serialize(System.Object,System.Type)
Serializes an object of a given type to a JSON string. 


#### SerializeAsync``1(``0,System.IO.Stream,System.Threading.CancellationToken)
Asynchronously serializes an object to a JSON string. 


#### SerializeAsync(System.Object,System.Type,System.IO.Stream,System.Threading.CancellationToken)
Asynchronously serializes an object of a given type to a JSON string. 


#### Deserialize``1(System.IO.Stream)
Deserializes a JSON stream to an object of type T. 


#### Deserialize(System.IO.Stream,System.Type)
Deserializes a JSON stream to an object of a given type. 


#### DeserializeAsync``1(System.IO.Stream,System.Threading.CancellationToken)
Asynchronously deserializes a JSON stream to an object of type T. 


#### DeserializeAsync(System.IO.Stream,System.Type,System.Threading.CancellationToken)
Asynchronously deserializes a JSON stream to an object of a given type. 


#### Deserialize``1(System.String)
Deserializes a JSON string to an object of type T. 


#### Deserialize(System.String,System.Type)
Deserializes a JSON string to an object of a given type. 


#### AsPropertyName(System.String)
Use the configured property naming policy to change the provided value. 


##### Parameters
* *propertyName:* The property name to convert.




##### Return value
The converted property name.



## Class: System.Text.SerializerTypes
Specifies different types of serialization formats. 

### Fields

#### Json
Represents the JSON serialization format.
#### Bson
Represents the BSON (Binary JSON) serialization format.
#### Xml
Represents the XML serialization format.

## Class: System.Text.Templating.FileTemplateSource
Access template from file system 

### Methods


#### Constructor
Access template from file system 


#### Get(System.String)
Look up templates from a file system. 


##### Parameters
* *templateName:* 




##### Return value




## Class: System.Text.Templating.FileTemplatingOptions
Configuration settings for file templating engine 

### Properties

#### TemplatePath
template source path
#### SandboxPath
sandbox root path
#### Priority
template priority

## Class: System.Text.Templating.TemplateContext
Represents the context of a template, providing information about the template and its processing. 

### Properties

#### TemplateName
Gets or sets the name of the template.
#### TemplateContentType
Gets or sets the content type of the template.
#### TemplateFileExtension
Gets or sets the file extension of the template.
#### TemplateSource
Gets or sets the source of the template.
#### TemplateReference
Gets or sets the reference identifier of the template.
#### OpenTemplate
Gets or sets the function to open the template as a stream.
#### TargetContentType
Gets or sets the content type of the target.
#### TargetFileExtension
Gets or sets the file extension of the target.
#### Priority
Gets or sets the priority of the template.
### Methods


#### ToString
Returns a string representation of the template context. 


##### Return value
A string representation of the template context.



## Class: System.Text.Templating.TemplateEngine
Generate templating engine that will try to use best match for source and provider 

### Methods


#### Constructor
Generate templating engine that will try to use best match for source and provider 


##### Parameters
* *sources:* An enumerable collection of template sources.
* *providers:* An enumerable collection of template providers.
* *logger:* The logger for capturing log messages.




#### ApplyAsync(System.String,System.Object,System.IO.Stream)
Applies the template asynchronously to the provided data and writes the result to the target stream. 


##### Parameters
* *templateName:* The name of the template to apply.
* *data:* The data to apply to the template.
* *target:* The stream to write the result to.




##### Return value
The applied template context or null if no matching template is found.



#### ApplyAsync(OoBDev.System.Text.Templating.ITemplateContext,System.Object,System.IO.Stream)
Applies the template asynchronously to the provided data and writes the result to the target stream. 


##### Parameters
* *context:* The template context to apply.
* *data:* The data to apply to the template.
* *target:* The stream to write the result to.




##### Return value
true if the template is applied successfully; otherwise, false.



#### ApplyAsync(System.String,System.Object)
Applies the template asynchronously to the provided data and returns the result as a string. 


##### Parameters
* *templateName:* The name of the template to apply.
* *data:* The data to apply to the template.




##### Return value
The applied template as a string or null if no matching template is found.



#### ApplyAsync(OoBDev.System.Text.Templating.ITemplateContext,System.Object)
Applies the template asynchronously to the provided data and returns the result as a string. 


##### Parameters
* *context:* The template context to apply.
* *data:* The data to apply to the template.




##### Return value
The applied template as a string or null if the template cannot be applied.



#### Get(System.String)
Gets the template context with the specified name. 


##### Parameters
* *templateName:* The name of the template to retrieve.




##### Return value
The template context or null if no matching template is found.



#### GetAll(System.String)
Gets all template contexts with the specified name. 


##### Parameters
* *templateName:* The name of the template to retrieve.




##### Return value
An enumerable collection of template contexts.



## Class: System.Text.Templating.XsltTemplateProvider
Provides template processing using XSLT (eXtensible Stylesheet Language Transformations). 

### Properties

#### SupportedContentTypes
Gets the collection of supported content types by the template provider. application/xslt+xml
### Methods


#### Constructor
Initializes a new instance of the 
 *See: T:OoBDev.System.Text.Templating.XsltTemplateProvider*class with the specified XML serializer. 


##### Parameters
* *xmlSerializer:* The XML serializer to be used by the provider.




#### CanApply(OoBDev.System.Text.Templating.ITemplateContext)
Determines whether this template provider can apply template processing to the given context. 


##### Parameters
* *context:* The template context.




##### Return value
true if the template processing can be applied; otherwise, false.



#### ApplyAsync(OoBDev.System.Text.Templating.ITemplateContext,System.Object,System.IO.Stream)
Applies the XSLT template associated with the specified context, using the provided data, and writes the result to the target stream asynchronously. 


##### Parameters
* *context:* The template context.
* *data:* The data to apply to the template.
* *target:* The stream where the result will be written.




##### Return value
A task representing the asynchronous operation, indicating whether the application was successful.



## Class: System.Text.Xml.Linq.XFragment
Represents a fragment of XML nodes with additional functionality for manipulation and conversion. 

### Properties

#### Count
Gets the number of elements contained in the collection of XML nodes.
#### IsReadOnly
Gets a value indicating whether the collection of XML nodes is read-only.
#### Item(System.Int32)
Gets or sets the XML node at the specified index. The zero-based index of the XML node to get or set.The XML node at the specified index.
### Methods


#### Constructor
Initializes a new instance of the 
 *See: T:OoBDev.System.Text.Xml.Linq.XFragment*class with the specified fragment. 


##### Parameters
* *fragment:* The fragment to initialize the instance with.




#### Constructor
Initializes a new instance of the 
 *See: T:OoBDev.System.Text.Xml.Linq.XFragment*class with the specified nodes. 


##### Parameters
* *nodes:* The nodes to initialize the instance with.




#### Constructor
Initializes a new instance of the 
 *See: T:OoBDev.System.Text.Xml.Linq.XFragment*class with the specified nodes. 


##### Parameters
* *nodes:* The nodes to initialize the instance with.




#### Constructor
Initializes a new instance of the 
 *See: T:OoBDev.System.Text.Xml.Linq.XFragment*class with the specified nodes. 


##### Parameters
* *node:* The nodes to initialize the instance with.
* *nodes:* The nodes to initialize the instance with.




#### Constructor
Initializes a new instance of the 
 *See: T:OoBDev.System.Text.Xml.Linq.XFragment*class with the specified XML string. 


##### Parameters
* *xml:* The XML string to initialize the instance with.




#### Constructor
Initializes a new instance of the 
 *See: T:OoBDev.System.Text.Xml.Linq.XFragment*class with the specified XML reader. 


##### Parameters
* *xmlReader:* The XML reader to initialize the instance with.




#### ToString
Returns the XML string representation of the fragment. 


##### Return value
The XML string representation of the fragment.



#### CreateReader
Creates an XML reader for the fragment. 


##### Return value
An XML reader for the fragment.



#### Parse(System.String)
Parses the specified XML string into an 
 *See: T:OoBDev.System.Text.Xml.Linq.XFragment*instance. 


##### Parameters
* *xml:* The XML string to parse.




##### Return value
An instance representing the parsed XML string.



#### Parse(System.Xml.XmlReader)
Parses the XML content from the specified reader into an 
 *See: T:OoBDev.System.Text.Xml.Linq.XFragment*instance. 


##### Parameters
* *xmlReader:* The XML reader containing the XML content to parse.




##### Return value
An instance representing the parsed XML content.



#### GetEnumerator
Returns an enumerator that iterates through the collection of XML nodes. 


##### Return value
An enumerator that can be used to iterate through the collection of XML nodes.



#### CreateNavigator
Creates an XPath navigator for navigating the fragment. 


##### Return value
An XPath navigator for navigating the fragment.



#### IndexOf(System.Xml.Linq.XNode)
Determines the index of a specific XML node in the collection. 


##### Parameters
* *item:* The XML node to locate in the collection.




##### Return value
The zero-based index of the XML node within the collection, if found; otherwise, -1.



#### Insert(System.Int32,System.Xml.Linq.XNode)
Inserts an XML node into the collection at the specified index. 


##### Parameters
* *index:* The zero-based index at which the XML node should be inserted.
* *item:* The XML node to insert into the collection.




#### RemoveAt(System.Int32)
Removes the XML node at the specified index from the collection. 


##### Parameters
* *index:* The zero-based index of the XML node to remove.




#### Add(System.Xml.Linq.XNode)
Adds an XML node to the end of the collection. 


##### Parameters
* *item:* The XML node to add to the collection.




#### Clear
Removes all XML nodes from the collection. 


#### Contains(System.Xml.Linq.XNode)
Determines whether the collection contains a specific XML node. 


##### Parameters
* *item:* The XML node to locate in the collection.




##### Return value
true if the XML node is found in the collection; otherwise, false.



#### CopyTo(System.Xml.Linq.XNode[],System.Int32)
Copies the elements of the collection to an array, starting at a particular index. 


##### Parameters
* *array:* The one-dimensional array that is the destination of the elements copied from the collection.
* *arrayIndex:* The zero-based index in the array at which copying begins.




#### Remove(System.Xml.Linq.XNode)
Removes the first occurrence of a specific XML node from the collection. 


##### Parameters
* *item:* The XML node to remove from the collection.




##### Return value
true if the XML node was successfully removed from the collection; otherwise, false.



#### op_Implicit(System.String)~OoBDev.System.Text.Xml.Linq.XFragment
Implicitly converts a string to an 
 *See: T:OoBDev.System.Text.Xml.Linq.XFragment*instance. 


##### Parameters
* *xml:* The XML string to convert.




#### op_Implicit(OoBDev.System.Text.Xml.Linq.XFragment)~System.String
Implicitly converts an 
 *See: T:OoBDev.System.Text.Xml.Linq.XFragment*instance to a string. 


##### Parameters
* *fragment:* The instance to convert.




#### op_Implicit(System.Xml.Linq.XNode[])~OoBDev.System.Text.Xml.Linq.XFragment
Implicitly converts an array of XML nodes to an 
 *See: T:OoBDev.System.Text.Xml.Linq.XFragment*instance. 


##### Parameters
* *nodes:* The array of XML nodes to convert.




## Class: System.Text.Xml.Serialization.DefaultXmlSerializer
Default XmlSerializer, 

### Fields

#### DefaultContentType
Gets the default content type for XML serialization.
### Properties

#### ContentType
Gets the content type used for XML serialization.
### Methods


#### Constructor
Initializes a new instance of the 
 *See: T:OoBDev.System.Text.Xml.Serialization.DefaultXmlSerializer*class. 


#### Deserialize``1(System.IO.Stream)
convert stream into object 


##### Parameters
* *stream:* 




##### Return value




#### Deserialize(System.IO.Stream,System.Type)
convert stream into object 


##### Parameters
* *stream:* 
* *type:* template model to deserialize




##### Return value




#### Deserialize``1(System.String)
convert stream into object 


##### Parameters
* *input:* 




##### Return value




#### Deserialize(System.String,System.Type)
convert stream into object 


##### Parameters
* *input:* 
* *type:* template model to deserialize




##### Return value




#### DeserializeAsync``1(System.IO.Stream,System.Threading.CancellationToken)
convert stream into object 


##### Parameters
* *stream:* 
* *cancellationToken:* 




##### Return value




#### DeserializeAsync(System.IO.Stream,System.Type,System.Threading.CancellationToken)
convert stream into object 


##### Parameters
* *stream:* 
* *type:* template model to deserialize
* *cancellationToken:* 




##### Return value




#### Serialize(System.Object,System.Type)
convert the object based on the type definition 


##### Parameters
* *obj:* object to serialize
* *type:* template model to serialize




##### Return value




#### Serialize``1(``0)
convert the object based on the type definition 


##### Parameters
* *obj:* object to serialize




##### Return value




#### SerializeAsync(System.Object,System.Type,System.IO.Stream,System.Threading.CancellationToken)
convert the object based on the type definition 


##### Parameters
* *obj:* object to serialize
* *type:* template model to serialize
* *stream:* 
* *cancellationToken:* 




##### Return value




#### SerializeAsync``1(``0,System.IO.Stream,System.Threading.CancellationToken)
convert the object based on the type definition 


##### Parameters
* *obj:* object to serialize
* *stream:* 
* *cancellationToken:* 




##### Return value


