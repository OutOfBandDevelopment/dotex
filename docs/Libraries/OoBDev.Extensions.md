# OoBDev.Extensions


## Class: Extensions.Accessors.Accessor`1
Represents an accessor for a value of type 
. 
The type of the value to be accessed. 

### Properties

#### Value
Gets or sets the value associated with this accessor.

## Class: Extensions.Configuration.CommandLine
builder pattern for command parameter arguments 

### Methods


#### AddParameters``1(System.Collections.Generic.IDictionary{System.String,System.String})
add additional configurable parameters 


##### Parameters
* *items:* 




##### Return value




#### BuildParameters``1
entry point or defining configurable parameters 


##### Return value




## Class: Extensions.Configuration.ConfigurationBuilderExtensions
Extension methods for adding in-memory collections to the 
 *See: T:Microsoft.Extensions.Configuration.IConfigurationBuilder*. 

### Methods


#### AddInMemoryCollection(Microsoft.Extensions.Configuration.IConfigurationBuilder,System.Collections.Generic.IEnumerable{System.ValueTuple{System.String,System.String}})
Adds an in-memory collection to the 
 *See: T:Microsoft.Extensions.Configuration.IConfigurationBuilder*using the specified initial data. 


##### Parameters
* *configurationBuilder:* The to add the in-memory collection to.
* *initialData:* The initial data to populate the in-memory collection.




##### Return value
The modified .



#### AddInMemoryCollection(Microsoft.Extensions.Configuration.IConfigurationBuilder,System.ValueTuple{System.String,System.String},System.ValueTuple{System.String,System.String}[])
Adds an in-memory collection to the 
 *See: T:Microsoft.Extensions.Configuration.IConfigurationBuilder*using the specified initial data. 


##### Parameters
* *configurationBuilder:* The to add the in-memory collection to.
* *item:* The first item of the in-memory collection.
* *initialData:* Additional initial data to populate the in-memory collection.




##### Return value
The modified .



## Class: Extensions.IO.FileTools
Provides methods for working with files. 

### Methods


#### SplitFileAsync(System.String,System.Int32,System.Int32)
Asynchronously splits a file into chunks of specified length and overlap. 


##### Parameters
* *filename:* The path of the file to split.
* *chunkLength:* The length of each chunk.
* *overlap:* The overlap between consecutive chunks.




##### Return value
An asynchronous enumerable sequence of representing the chunks of the file.



## Class: Extensions.IO.StreamExtensions
Provides methods for working with streams. 

### Fields

#### DefaultChunkLength
The default length of context used when splitting files.
#### DefaultOverlap
The default overlap between chunks when splitting files.
### Methods


#### CopyOf(System.IO.Stream)
Create an in memory copy of the provided stream 


##### Parameters
* *stream:* stream to copy




##### Return value
copy of stream



#### SplitStreamAsync(System.IO.Stream,System.Int32,System.Int32)
Asynchronously splits a file into chunks of specified length and overlap. 


##### Parameters
* *stream:* The stream to split.
* *contextLength:* The length of each chunk.
* *overlap:* The overlap between consecutive chunks.




##### Return value
An asynchronous enumerable sequence of representing the chunks of the file.



## Class: Extensions.IO.StreamJsonDeserializeExtensions
Set of extension method to centralize deserialize of stream using System.Text.Json 

### Methods


#### AsJsonAsync``1(System.IO.Stream,System.Text.Json.JsonSerializerOptions)
Convert JSON Stream to .Net Object 


##### Parameters
* *stream:* 
* *options:* 




##### Return value




#### AsJsonAsync(System.IO.Stream,System.Type,System.Text.Json.JsonSerializerOptions)
Convert JSON Stream to .Net Object 


##### Parameters
* *stream:* 
* *type:* 
* *options:* 




##### Return value




#### AsJson``1(System.IO.Stream,System.Text.Json.JsonSerializerOptions)
Convert JSON Stream to .Net Object 


##### Parameters
* *stream:* 
* *options:* 




##### Return value




#### AsJson(System.IO.Stream,System.Type,System.Text.Json.JsonSerializerOptions)
Convert JSON Stream to .Net Object 


##### Parameters
* *stream:* 
* *type:* 
* *options:* 




##### Return value




## Class: Extensions.IO.StreamXmlDeserializeExtensions
Set of extension method to centralize deserialize of stream using System.Xml 

### Methods


#### AsXmlAsync``1(System.IO.Stream,System.Type[])
Convert XML Stream to .Net Object 


##### Parameters
* *stream:* 
* *extraTypes:* 




##### Return value




#### AsXmlAsync(System.IO.Stream,System.Type,System.Type[])
Convert XML Stream to .Net Object 


##### Parameters
* *stream:* 
* *type:* 
* *extraTypes:* 




##### Return value




#### AsXml``1(System.IO.Stream,System.Type[])
Convert XML Stream to .Net Object 


##### Parameters
* *stream:* 
* *extraTypes:* 




##### Return value




#### AsXml(System.IO.Stream,System.Type,System.Type[])
Convert XML Stream to .Net Object 


##### Parameters
* *stream:* 
* *type:* 
* *extraTypes:* 




##### Return value




## Class: Extensions.Linq.AsyncEnumerableExtensions
Extensions to add async support to existing IEnumerable{T} 

### Methods


#### ToListAsync``1(System.Collections.Generic.IAsyncEnumerable{``0})
Converts an asynchronous enumerable sequence to a set (IEnumerable{T}) asynchronously. 


##### Parameters
* *items:* The asynchronous enumerable sequence to convert.




##### Return value
A task representing the asynchronous operation. The task result contains the set of elements in the sequence.



#### ToListAsync``1(System.Linq.IQueryable{``0},System.Threading.CancellationToken)
Process IQueryable{T} to a List{T} as async 


##### Parameters
* *source:* 
* *cancellationToken:* 




##### Return value




#### AsEnumerableAsync``1(System.Collections.Generic.IAsyncEnumerable{``0},System.Threading.CancellationToken)
Convert IAsyncEnumerable{TModel} to Task{IEnumerable{TModel}} 


##### Parameters
* *items:* 
* *token:* 




##### Return value




#### AsAsyncEnumerable``1(System.Threading.Tasks.Task{System.Collections.Generic.IEnumerable{``0}},System.Threading.CancellationToken)
Convert Task{IEnumerable{TModel}} to IAsyncEnumerable{TModel} 


##### Parameters
* *items:* 
* *cancellationToken:* 




##### Return value




#### AsAsyncEnumerable``1(System.Collections.Generic.IEnumerable{``0},System.Threading.CancellationToken)
Convert IEnumerable{TModel} to IAsyncEnumerable{TModel} 


##### Parameters
* *items:* 
* *cancellationToken:* 




##### Return value




#### Select``2(System.Collections.Generic.IAsyncEnumerable{``0},System.Func{``0,``1})
Project each element of the source async enumerable sequence into a new form asynchronously. 


##### Parameters
* *items:* The source async enumerable sequence to project.
* *map:* A transform function to apply to each element.




##### Return value
An async enumerable sequence whose elements are the result of invoking the transform function on each element of the source.



#### Select``2(System.Collections.Generic.IAsyncEnumerable{``0},System.Func{``0,System.Threading.Tasks.Task{``1}})
Project each element of the source async enumerable sequence into a new form asynchronously. 


##### Parameters
* *items:* The source async enumerable sequence to project.
* *map:* A transform function to apply to each element asynchronously.




##### Return value
An async enumerable sequence whose elements are the result of invoking the transform function on each element of the source.



#### ToReadOnlyCollectionAsync``1(System.Collections.Generic.IAsyncEnumerable{``0},System.Threading.CancellationToken)
Converts an async enumerable sequence to a read-only collection asynchronously. 


##### Parameters
* *items:* The source async enumerable sequence to convert.
* *cancellationToken:* A cancellation token that can be used to cancel the operation.




##### Return value
A task representing the asynchronous operation. The task result contains a read-only collection of elements.



#### ToSetAsync``1(System.Collections.Generic.IAsyncEnumerable{``0})
Converts an asynchronous enumerable sequence to a set (IEnumerable{T}) asynchronously. 


##### Parameters
* *items:* The asynchronous enumerable sequence to convert.




##### Return value
A task representing the asynchronous operation. The task result contains the set of elements in the sequence.



## Class: Extensions.Linq.DictionaryExtensions
Reusable extensions for Generic Dictionaries 

### Methods


#### TryGetValue``2(System.Collections.Generic.IDictionary{``0,``1},``0,``1@,System.Collections.Generic.IEqualityComparer{``0})
extend try get value to allow for using a different IEqualityComparer{TKey} 


##### Parameters
* *dictionary:* 
* *key:* 
* *value:* 
* *comparer:* 




##### Return value




#### ChangeComparer``2(System.Collections.Generic.IDictionary{``0,``1},System.Collections.Generic.IEqualityComparer{``0})
Rebuild dictionary to use a different IEqualityComparer{TKey} 


##### Parameters
* *dictionary:* 
* *comparer:* 




##### Return value




## Class: Extensions.Reflection.ReflectionExtensions
Extensions for reflection and common patterns. 

### Fields

#### PublicProperties
flag combination to select public instance properties
#### PublicStaticMethod
flag combination to select public static methods
#### PublicInstanceMethod
flag combination to select public instance methods
### Methods


#### GetKeyValue(System.Object)
lookup key values for provided entity 


##### Parameters
* *item:* 




##### Return value




#### MakeSafeArray(System.Type,System.Array,OoBDev.System.ResponseModel.ICaptureResultMessage)
safely create new array for a given element type. 


##### Parameters
* *type:* target conversion type
* *inputs:* source value
* *capture:* capture message




##### Return value




#### MakeSafe(System.Type,System.Object,OoBDev.System.ResponseModel.ICaptureResultMessage)
Make safe will try to convert input to target type as best as possible. 


##### Parameters
* *type:* target conversion type
* *input:* source value
* *capture:* capture message




##### Return value




#### TryParse(System.Type,System.String,System.Object@,OoBDev.System.ResponseModel.ICaptureResultMessage)
Use best possible match for parsing to provided type 


##### Parameters
* *type:* target conversion type
* *toParse:* source value
* *parsed:* output value
* *capture:* capture message




##### Return value




#### GetShortTypeName(System.Type)
Type name with full namespace, name and assembly. version is not included 


##### Parameters
* *type:* 




##### Return value




#### GetAttributes(System.Type)
Get all attributes for type 


##### Parameters
* *type:* 




##### Return value




#### GetAttributes``1(System.Type)
Get all attributes of selected style for reflected type 


##### Parameters
* *type:* 




##### Return value




#### GetPropertiesByAttribute``1(System.Type)
get property info where attribute matches 


##### Parameters
* *type:* 




##### Return value




#### GetStaticMethod(System.Type,System.String,System.Type[])
get static method 


##### Parameters
* *type:* 
* *methodName:* 
* *parameterTypes:* 




##### Return value




#### GetInstanceMethod(System.Type,System.String,System.Type[])
get instance method 


##### Parameters
* *type:* 
* *methodName:* 
* *parameterTypes:* 




##### Return value




#### GetParametersTypes(System.Reflection.MethodInfo)
get parameters for method 


##### Parameters
* *method:* 




##### Return value




## Class: Extensions.Reflection.ResourceExtensions
Set of extension methods for embedded resources 

### Methods


#### GetResourceStream(System.Object,System.String)
Lookup resource stream based on filename relative the scope of context 


##### Parameters
* *context:* 
* *resourceName:* 




##### Return value




#### GetResourceStream(System.Type,System.String)
Lookup resource stream based on filename relative the scope of Type 


##### Parameters
* *type:* 
* *resourceName:* 




##### Return value




#### GetResourceStream(System.Reflection.Assembly,System.String)
Lookup resource stream based on filename relative the scope of Type 


##### Parameters
* *assembly:* 
* *resourceName:* 




##### Return value




#### GetResourceAsStringAsync(System.Object,System.String)
Lookup resource content based on filename relative the scope of context 


##### Parameters
* *context:* 
* *resourceName:* 




##### Return value




#### GetResourceAsString(System.Object,System.String)
Lookup resource content based on filename relative the scope of context 


##### Parameters
* *context:* 
* *resourceName:* 




##### Return value




## Class: Extensions.ServiceCollectionExtensions
Suggested IOC configurations 

### Methods


#### AddAccessor``1(Microsoft.Extensions.DependencyInjection.IServiceCollection)
Register accessor type that is scoped to as AsyncLocal 


##### Parameters
* *services:* 




##### Return value




#### GetSingletonInstance``2(Microsoft.Extensions.DependencyInjection.IServiceCollection,``1@)
Get singleton instance from IOC container 


##### Parameters
* *services:* 
* *instance:* 




##### Return value




## Class: Extensions.StringTools
Provides utility methods for string manipulation. 

### Methods


#### SplitBy(System.String,System.Int32,System.Char)
Splits the input string into lines of specified length, breaking at the nearest space character. 


##### Parameters
* *input:* The input string to split.
* *length:* The maximum length of each line (default is 80).
* *breaker:* The character at which to break lines (default is space).




##### Return value
An enumerable of strings representing the split lines.



#### WriteAsLines(System.Collections.Generic.IEnumerable{System.String})
Concatenates the lines into a single string with newline separators. 


##### Parameters
* *lines:* The lines to concatenate.




##### Return value
A single string with newline separators.

