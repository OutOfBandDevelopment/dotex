# OoBDev.System.Abstractions


## Class: System.Accessors.IAccessor`1
IAccessor[T] is a type that allows for a instance to be bound to a async context 

### Properties

#### Value
accessible value

## Class: System.ComponentModel.EndStateAttribute
this attribute tags valid end states for enum based state machines 


## Class: System.ComponentModel.EnumValueAttribute
output enum string as this value when serialized as json 

### Properties

#### Name
value to output in place of Enum.ToString() with Json Serializer
### Methods


#### Constructor
output enum string as this value when serialized as json 


## Class: System.ComponentModel.ExcludeFromUniqueAttribute
Attribute used to exclude an enum or enum field from being considered for uniqueness checks. 


## Class: System.ComponentModel.IVersionProvider
Provides information about the version of an assembly. 

### Properties

#### Title
Gets the title of the assembly.
#### Version
Gets the version of the assembly.
#### Description
Gets the description of the assembly.
#### Assembly
Gets the associated with the version information.

## Class: System.ComponentModel.Search.DefaultSortAttribute
part of default sort for entity 

### Properties

#### TargetName
property name to use for mapping
#### Priority
sort column position priority
#### Order
direction to order this mapped column
### Methods


#### Constructor
Initializes a new instance of the 
 *See: T:OoBDev.System.ComponentModel.Search.DefaultSortAttribute*class. 


##### Parameters
* *targetName:* The property name to use for mapping.
* *priority:* The sort column position priority.
* *order:* The direction to order this mapped column.




#### Constructor
Initializes a new instance of the 
 *See: T:OoBDev.System.ComponentModel.Search.DefaultSortAttribute*class. 


## Class: System.ComponentModel.Search.FilterableAttribute
Allow tagging entity classes to enumerate filterable fields/properties. 

### Properties

#### TargetName
column mapping override
### Methods


#### Constructor
Initializes a new instance of the 
 *See: T:OoBDev.System.ComponentModel.Search.FilterableAttribute*class. 


#### Constructor
Initializes a new instance of the 
 *See: T:OoBDev.System.ComponentModel.Search.FilterableAttribute*class. 


## Class: System.ComponentModel.Search.IgnoreStringComparisonReplacementAttribute
exclude from string comparison replacement visitor 


## Class: System.ComponentModel.Search.ISearchQueryIntercept
Provide entry point to commonly intercept and override search definitions. Example 
 *See: T:OoBDev.System.ComponentModel.Search.SearchTermDefaultAttribute*
### Methods


#### Intercept(OoBDev.System.Linq.Search.ISearchQuery)
modify or pass though search query before processing. 


##### Parameters
* *searchQuery:* 




##### Return value




## Class: System.ComponentModel.Search.NotFilterableAttribute
Specifies that a property or class should be explicitly excluded from filter selection. 

### Properties

#### TargetName
Gets the target name to be excluded from filter selection.
### Methods


#### Constructor
Initializes a new instance of the 
 *See: T:OoBDev.System.ComponentModel.Search.NotFilterableAttribute*class with a target name. 


##### Parameters
* *targetName:* The target name to be excluded from filter selection.




#### Constructor
Initializes a new instance of the 
 *See: T:OoBDev.System.ComponentModel.Search.NotFilterableAttribute*class without a target name. 


## Class: System.ComponentModel.Search.NotSearchableAttribute
explicitly exclude properties from search 

### Properties

#### TargetName
Target name required only if this is used on the class
### Methods


#### Constructor
explicitly exclude properties from search 


##### Parameters
* *targetName:* virtual property to target




#### Constructor
explicitly exclude properties from search 


## Class: System.ComponentModel.Search.NotSortableAttribute
Specifies that a property or class should not be sortable. 

### Properties

#### TargetName
Gets the name of the target property that should not be sortable.
### Methods


#### Constructor
Initializes a new instance of the 
 *See: T:OoBDev.System.ComponentModel.Search.NotSortableAttribute*class. 


##### Parameters
* *targetName:* The name of the target property that should not be sortable.




#### Constructor
Initializes a new instance of the 
 *See: T:OoBDev.System.ComponentModel.Search.NotSortableAttribute*class. 


## Class: System.ComponentModel.Search.SearchableAttribute
include field to be searchable for "SearchTerm" 

### Properties

#### TargetName
Target name required only if this is used on the class
### Methods


#### Constructor
mark a virtual property as searchable 


##### Parameters
* *targetName:* 




#### Constructor
mark a property as searchable 


## Class: System.ComponentModel.Search.SearchTermDefaultAttribute
provide the ability to control how search terms are handled if not wilded carded 

### Properties

#### Default
rule to use for provided search term if not quoted
### Methods


#### Constructor
provide the ability to control how search terms are handled if not wilded carded 


#### Intercept(OoBDev.System.Linq.Search.ISearchQuery)
use the `Default` to control pattern for searches without provided wild cards 


##### Parameters
* *searchQuery:* 




##### Return value




## Class: System.ComponentModel.Search.SearchTermDefaults
Specifies default search term options for comparison. 

### Fields

#### EqualTo
Represents an equal comparison for search terms.
#### Contains
Represents a contains comparison for search terms.
#### StartsWith
Represents a starts-with comparison for search terms.
#### EndsWith
Represents an ends-with comparison for search terms.

## Class: System.ConfigurationMissingException
Represents an exception thrown when a required configuration is missing. 

This exception is typically thrown when attempting to access a configuration value that is not present.
### Methods


#### Constructor
This constructor sets the exception message to indicate the missing configuration key.
Initializes a new instance of the 
 *See: T:OoBDev.System.ConfigurationMissingException*class with the specified configuration key. 


##### Parameters
* *configurationKey:* The key of the missing configuration.




## Class: System.Configuration.CommandParameterAttribute
Specifies that a property represents a command parameter. 

### Properties

#### Short
Gets or sets the short representation of the command parameter.
#### Value
Gets or sets the value of the command parameter.
#### TypeId
Gets a unique identifier for this attribute.

## Class: System.IO.ContentChunk
Represents a chunk of content along with metadata. 

### Properties

#### Data
Gets the data of the content chunk.
#### Sequence
Gets the sequence number of the chunk within the original content.
#### Start
Gets the starting position of the chunk within the original content.
#### Length
Gets the length of the chunk.
### Methods


#### Constructor
Initializes a new instance of the 
 *See: T:OoBDev.System.IO.ContentChunk*class. 


##### Parameters
* *data:* The data of the content chunk.
* *sequence:* The sequence number of the chunk within the original content.
* *start:* The starting position of the chunk within the original content.
* *length:* The length of the chunk.




## Class: System.IO.FileMetaData
Represents metadata associated with a file. 

### Properties

#### Uuid
Gets the universally unique identifier (UUID) of the file.
#### Path
Gets the path of the file.
#### Hash
Gets the hash value of the file.
#### BasePath
Gets the base path of the file.
### Methods


#### Constructor
Initializes a new instance of the 
 *See: T:OoBDev.System.IO.FileMetaData*record. 


##### Parameters
* *uuid:* The universally unique identifier (UUID) of the file.
* *path:* The path of the file.
* *hash:* The hash value of the file.
* *basePath:* The base path of the file.




## Class: System.IO.ITempFile
temp file handle 

### Properties

#### FilePath
path to temp file
### Methods


#### 
Get a managed temporary file. ITempFile will be deleted when disposed. 


##### Return value




## Class: System.IO.ITempFileFactory
this is a provider for managed temp files 

### Methods


#### GetTempFile
Get a managed temporary file. ITempFile will be deleted when disposed. 


##### Return value




## Class: System.Linq.Expressions.IExpressionTreeBuilder
Represents a builder for constructing expression trees used in querying and filtering. 

### Methods


#### GetSearchablePropertyNames
Gets the names of properties that can be used for searching. 


##### Return value
The collection of searchable property names.



#### GetSortablePropertyNames
Gets the names of properties that can be used for sorting. 


##### Return value
The collection of sortable property names.



#### GetFilterablePropertyNames
Gets the names of properties that can be used for filtering. 


##### Return value
The collection of filterable property names.



#### DefaultSortOrder
Gets the default sort order defined by the expression tree builder. 


##### Return value
The collection of default sort order information.



#### 
Gets the predicate expression based on the provided filter parameters. 


##### Parameters
* *name:* The name of the property to filter on.
* *value:* The filter parameter value.
* *stringComparison:* The string comparison option for string-based filtering.
* *isSearchTerm:* A flag indicating whether the filter is a search term.




##### Return value
The predicate expression based on the filter parameters.



#### 
Builds the expression tree based on the provided query parameter. 


##### Parameters
* *queryParameter:* The query parameter used for building the expression tree.
* *stringComparison:* The string comparison option for string-based filtering.
* *isSearchTerm:* A flag indicating whether the query parameter is a search term.




##### Return value
The expression tree representing the filtering condition.



#### 
Gets a dictionary of property names and their corresponding expression trees. 


##### Return value
The dictionary of property names and expression trees.



## Class: System.Linq.Expressions.IExpressionTreeBuilder`1
Represents a typed builder for constructing expression trees used in querying and filtering. 
The type of the model for which expression trees are built. 

### Methods


#### GetPredicateExpression(System.String,OoBDev.System.Linq.Search.FilterParameter,System.StringComparison,System.Boolean)
Gets the predicate expression based on the provided filter parameters. 


##### Parameters
* *name:* The name of the property to filter on.
* *value:* The filter parameter value.
* *stringComparison:* The string comparison option for string-based filtering.
* *isSearchTerm:* A flag indicating whether the filter is a search term.




##### Return value
The predicate expression based on the filter parameters.



#### BuildExpression(System.Object,System.StringComparison,System.Boolean)
Builds the expression tree based on the provided query parameter. 


##### Parameters
* *queryParameter:* The query parameter used for building the expression tree.
* *stringComparison:* The string comparison option for string-based filtering.
* *isSearchTerm:* A flag indicating whether the query parameter is a search term.




##### Return value
The expression tree representing the filtering condition.



#### PropertyExpressions
Gets a dictionary of property names and their corresponding expression trees. 


##### Return value
The dictionary of property names and expression trees.



## Class: System.Linq.Expressions.IPostBuildExpressionVisitor
Represents an interface for a visitor that processes an expression tree after it has been built. 

### Methods


#### Visit(System.Linq.Expressions.Expression)
Visits the provided expression node after the expression tree has been built. 


##### Parameters
* *node:* The expression node to visit.




##### Return value
The modified expression node.



## Class: System.Linq.Search.FilterParameter
Filter parameter 

### Properties

#### EqualTo
`Equal To`: pass in the value to match for a given property If you are using string values you may also use wild cards \*bc -&gt; Ends with \*b\* -&gt; Contains ab\* -&gt; Starts with
#### NotEqualTo
`Not Equal To`: pass in the value to match for a given property If you are using string values you may also use wild cards \*bc -&gt; Ends with \*b\* -&gt; Contains ab\* -&gt; Starts with
#### InSet
This allows for providing a set of values where the value from the queries data must match at least one of provided values
#### GreaterThan
`Greater than`
#### GreaterThanOrEqualTo
`Greater than or equal to`
#### LessThan
`Less than`
#### LessThanOrEqualTo
`Less than or equal to`
### Methods


#### ToString
convert FilterParameter to string 


##### Return value




## Class: System.Linq.Search.IFilterQuery
Represents a query with filtering options. 

### Properties

#### Filter
Gets the collection of filter parameters.

## Class: System.Linq.Search.IPageQuery
Represents a page query for paginating results. 

### Properties

#### CurrentPage
Gets the current page number.
#### PageSize
Gets the size of each page.
#### ExcludePageCount
Gets a value indicating whether to exclude the total page count from the result.

## Class: System.Linq.Search.IQueryBuilder
Represents a query builder for executing queries with search parameters. 

### Methods


#### ExecuteBy(System.Linq.IQueryable,OoBDev.System.Linq.Search.ISearchQuery)
Executes a query using the specified 
*query*and 
*searchQuery*. 


##### Parameters
* *query:* The queryable data source.
* *searchQuery:* The search parameters for filtering and sorting the data.




##### Return value
An containing the result of the executed query.



#### 
Executes a typed query using the specified 
*query*and 
*searchQuery*. 


##### Parameters
* *query:* The typed queryable data source.
* *searchQuery:* The search parameters for filtering and sorting the data.




##### Return value
An containing the result of the executed query.



## Class: System.Linq.Search.IQueryBuilder`1
Represents a typed query builder for executing queries with search parameters. 
The type of the model in the query. 

### Methods


#### ExecuteBy(System.Linq.IQueryable{`0},OoBDev.System.Linq.Search.ISearchQuery)
Executes a typed query using the specified 
*query*and 
*searchQuery*. 


##### Parameters
* *query:* The typed queryable data source.
* *searchQuery:* The search parameters for filtering and sorting the data.




##### Return value
An containing the result of the executed query.



## Class: System.Linq.Search.ISearchQuery
Represents a query object that combines page, sort, search term, and filter criteria for searching data. 


## Class: System.Linq.Search.ISearchQuery`1
Represents a generic query object that combines page, sort, search term, and filter criteria for searching data of type 
. 
The type of the model. 


## Class: System.Linq.Search.ISearchTermQuery
Represents a query object that includes a search term for filtering data. 

### Properties

#### SearchTerm
Gets the search term used for filtering data.

## Class: System.Linq.Search.ISortBuilder`1
Represents an interface for building sorting expressions and applying sorting to a query. 
The type of the model. 

### Methods


#### SortBy(System.Linq.IQueryable{`0},OoBDev.System.Linq.Search.ISortQuery,OoBDev.System.Linq.Expressions.IExpressionTreeBuilder{`0},System.StringComparison)
Sorts the specified query based on the provided sort criteria. 


##### Parameters
* *query:* The query to be sorted.
* *searchRequest:* The sort criteria specified in the search request.
* *treeBuilder:* The expression tree builder for the model.
* *stringComparison:* The string comparison method used for sorting.




##### Return value
An representing the sorted query.



## Class: System.Linq.Search.ISortQuery
Represents an interface for specifying sorting criteria in a query. 

### Properties

#### OrderBy
Gets a dictionary containing sorting information, where the key is the column name and the value is the sort direction.

## Class: System.Linq.Search.OrderDirections
Enumeration to control sort order 

### Fields

#### Ascending
sort related items in ascending order
#### Descending
sort related items in descending order
#### 
Represents the ascending order direction.
#### 
Represents a short form of the ascending order direction.
#### 
Represents the descending order direction.
#### 
Represents a short form of the descending order direction.

## Class: System.Linq.Search.OrderDirectionsConstants
Provides constants for order directions. 

### Fields

#### Ascending
Represents the ascending order direction.
#### AscendingShort
Represents a short form of the ascending order direction.
#### Descending
Represents the descending order direction.
#### DescendingShort
Represents a short form of the descending order direction.

## Class: System.Linq.Search.SearchQuery`1
Represents a search query with filtering and sorting options for a specific model. 
The type of the model. 


## Class: System.Linq.Search.SearchQuery
Represents a generic search query with filtering and sorting options. 

### Properties

#### CurrentPage
Gets or sets the current page number.
#### PageSize
Gets or sets the page size.
#### ExcludePageCount
Gets or sets a value indicating whether to exclude page count from the result.
#### SearchTerm
Gets or sets the search term.
#### Filter
Gets or initializes the filters for the query.
#### OrderBy
Gets or initializes the sorting options for the query.
### Methods


#### ToString
Generates a string representation of the search query. 


##### Return value
A string representation of the search query.



## Class: System.Net.Http.CorrelationInfo
Represents information related to correlation and tracking in a system. 

### Properties

#### CorrelationId
Gets or sets the correlation identifier. The correlation identifier is used to associate related operations or events across different components or services.
#### RequestId
Gets or sets the request identifier. The request identifier is used to uniquely identify a specific request or operation within a system.

## Class: System.Net.Http.DefinedHttpHeaders
Contains constant values for commonly used HTTP headers. 

### Fields

#### CorrelationIdHeader
Represents the "X-Correlation-ID" HTTP header used for correlation identification.
#### RequestIdHeader
Represents the "X-Request-ID" HTTP header used for request identification.
#### AcceptLanguage
Represents the "Accept-Language" HTTP header used for specifying acceptable languages for the response.
#### ContentLanguage
Represents the "Content-Language" HTTP header used for specifying the language of the content.

## Class: System.Net.Mime.ContentTypesExtensions
Provides constants representing various content types. 

### Fields

#### Text.HandlebarsTemplate
Represents the content type for Handlebars templates.
#### Text.Calendar
Represents the content type for calendar data.
#### Text.Html
Represents the content type for HTML.
#### Text.Markdown
Represents the content type for Markdown.
#### Text.Yaml
Represents the content type for Yaml.
#### Text.Plain
Represents the content type for Plain text.
#### Application.XSLT
Represents the content type for XSLT (XML Stylesheet Language Transformations).

## Class: System.Net.Mime.ContentTypesExtensions.Text
Represents text-based content types. 

### Fields

#### HandlebarsTemplate
Represents the content type for Handlebars templates.
#### Calendar
Represents the content type for calendar data.
#### Html
Represents the content type for HTML.
#### Markdown
Represents the content type for Markdown.
#### Yaml
Represents the content type for Yaml.
#### Plain
Represents the content type for Plain text.

## Class: System.Net.Mime.ContentTypesExtensions.Application
Represents application-based content types. 

### Fields

#### XSLT
Represents the content type for XSLT (XML Stylesheet Language Transformations).

## Class: System.Providers.IDateTimeProvider
Provides date and time functionality. 

### Properties

#### Now
Gets the current local date and time. This property returns the current local date and time.
#### UtcNow
Gets the current Coordinated Universal Time (UTC) date and time. This property returns the current Coordinated Universal Time (UTC) date and time.

## Class: System.Providers.IGuidProvider
Represents a provider for generating and handling GUIDs. 

### Properties

#### Empty
Gets an empty GUID. This property returns a GUID with all bits set to zero.
### Methods


#### NewGuid
Generates a new GUID. 


##### Return value
A new GUID.



## Class: System.Reflection.EnumExtensions
Provides extension methods for working with enumerations. 

### Methods


#### AsString``1(``0)
Converts an enumeration value to its associated string representation. 


##### Parameters
* *input:* The enumeration value.




##### Return value
The string representation of the enumeration value.



#### ToEnum``1(System.Int32)
Converts an integer value to an enumeration value. 


##### Parameters
* *input:* The integer value.




##### Return value
The corresponding enumeration value.



#### ToEnum``1(System.String)
Converts a string to an enumeration value. 


##### Parameters
* *input:* The string representation of the enumeration value.




##### Return value
The corresponding enumeration value.



#### AsModel``1(``0)
Gets the enumeration model for a specific enumeration value. 


##### Parameters
* *enum:* The enumeration value.




##### Return value
The enumeration model.



#### AsModels``1
Gets a collection of enumeration models for all values of a specific enumeration type. 


##### Return value
A collection of enumeration models.



## Class: System.Reflection.IEnumModel
Represents a generic interface for providing information about an enumeration value. 

### Properties

#### Code
Gets the code associated with the enumeration value.
#### Description
Gets the description associated with the enumeration value.
#### Id
Gets the numerical identifier of the enumeration value.
#### IsEndState
Gets a value indicating whether the enumeration value is an end state.
#### IsExcludeFromUnique
Gets a value indicating whether the enumeration value is excluded from uniqueness checks.
#### Name
Gets the name of the enumeration value.
#### Order
Gets the order of the enumeration value.
#### Value
Gets the underlying value of the enumeration.
#### PossibleNames
Gets a collection of possible names for the enumeration value.
#### 
Gets the strongly-typed value of the enumeration.

## Class: System.Reflection.IEnumModel`1
Represents a generic interface for providing information about an enumeration value of type 
. 
The enumeration type. 

### Properties

#### Value
Gets the strongly-typed value of the enumeration.

## Class: System.Reflection.IResolveType
Represents an interface for resolving a 
 *See: T:System.Type*. 

### Methods


#### ResolveType
Resolves and returns the associated 
 *See: T:System.Type*. 


##### Return value
The resolved .



## Class: System.ResponseModel.CaptureResultMessage
Implementation of 
 *See: T:OoBDev.System.ResponseModel.ICaptureResultMessage*for capturing and publishing result messages. 

### Properties

#### Default
Default instance for CaptureResultMessage
### Methods


#### Publish(OoBDev.System.ResponseModel.ResultMessage[])
Publishes result messages to the capture stack. 


##### Parameters
* *resultMessages:* The result messages to be published.




#### Peek
Peeks at the current contents of the capture stack without clearing it. 


##### Return value
The current result messages in the capture stack.



#### Clear
Gets the default instance of 
 *See: T:OoBDev.System.ResponseModel.ICaptureResultMessage*. 


#### Capture
Gets the default instance of 
 *See: T:OoBDev.System.ResponseModel.ICaptureResultMessage*. 


## Class: System.ResponseModel.ICaptureResultMessage
Represents an interface for capturing and publishing result messages. 

### Methods


#### Publish(OoBDev.System.ResponseModel.ResultMessage[])
Publishes the specified result messages. 


##### Parameters
* *resultMessage:* The result messages to publish.




#### Peek
Peeks at the captured result messages without removing them. 


##### Return value
An IReadOnlyCollection of result messages.



#### Clear
Clears the captured result messages. 


#### Capture
Captures and returns the result messages, removing them from the capture. 


##### Return value
An IReadOnlyCollection of captured result messages.



## Class: System.ResponseModel.IModelResult
Represents a generic interface for a result containing model data and messages. 

### Properties

#### Data
Gets the data associated with the result.
#### Messages
Gets a collection of messages associated with the result.
#### 
Gets the strongly-typed data associated with the result.

## Class: System.ResponseModel.IModelResult`1
Represents a generic interface for a result containing model data of type 
and messages. 
The type of the model data. 

### Properties

#### Data
Gets the strongly-typed data associated with the result.

## Class: System.ResponseModel.IPagedQueryResult
Represents a generic interface for a paged query result containing model data and additional paging information. 

### Properties

#### CurrentPage
Gets the current page number.
#### TotalPageCount
Gets the total number of pages.
#### TotalRowCount
Gets the total number of rows across all pages.

## Class: System.ResponseModel.IPagedQueryResult`1
Represents a generic interface for a paged query result containing model data of type 
and additional paging information. 
The type of the model data. 


## Class: System.ResponseModel.IQueryResult
Represents a generic interface for a query result containing rows of data. 

### Properties

#### Rows
Gets the collection of rows in the query result.
#### 
Gets the collection of rows in the query result, typed as .

## Class: System.ResponseModel.IQueryResult`1
Represents a generic interface for a query result containing model data of type 
. 
The type of the model data. 

### Properties

#### Rows
Gets the collection of rows in the query result, typed as .

## Class: System.ResponseModel.IResult
Represents a result, which may include a collection of messages. 

### Properties

#### Messages
Gets a collection of result messages.

## Class: System.ResponseModel.MessageLevels
response message level 

### Fields

#### Trace
extra detailed information
#### Debug
information to assist in troubleshooting
#### Information
extra information about process
#### Warning
notifications and assumptions about processing that did not effect output
#### Error
errors that caused the system to not complete your request as you may have expected
#### Critical
information about processing that failed
#### None
unknown value

## Class: System.ResponseModel.ModelResult`1
Represents a result containing a single model. 
The type of the model. 

### Properties

#### Data
Gets the model data.
#### OoBDev#System#ResponseModel#IModelResult#Data
Gets the model data as an object.
#### Messages
Gets or sets the collection of result messages.
### Methods


#### Constructor
Initializes a new instance of the 
 *See: T:OoBDev.System.ResponseModel.ModelResult`1*class with the specified data. 


##### Parameters
* *data:* The model data.




## Class: System.ResponseModel.PagedQueryResult`1
Represents the result of a paged query, including information about the current page, total page count, total row count, and a collection of items. 
The type of the items in the result. 

### Properties

#### CurrentPage
Gets the total number of pages.
#### TotalPageCount
Gets the total number of pages.
#### TotalRowCount
Gets the total number of rows.
### Methods


#### Constructor
Initializes a new instance of the 
 *See: T:OoBDev.System.ResponseModel.PagedQueryResult`1*class. 


##### Parameters
* *currentPage:* The current page number.
* *totalPageCount:* The total number of pages.
* *totalRowCount:* The total number of rows.
* *items:* The collection of items in the result.




#### Constructor
Initializes a new instance of the 
 *See: T:OoBDev.System.ResponseModel.PagedQueryResult`1*class by wrapping an existing 
 *See: T:OoBDev.System.ResponseModel.IPagedQueryResult`1*instance. 


##### Parameters
* *toWrap:* The instance to wrap.




#### Constructor
Gets the total number of pages. 


## Class: System.ResponseModel.QueryResult`1
Represents the result of a query operation, containing a collection of items and optional result messages. 
The type of items in the result. 

### Properties

#### Rows
Gets the collection of items in the result.
#### OoBDev#System#ResponseModel#IQueryResult#Rows
Gets the collection of items in the result as a non-generic enumerable.
#### Messages
Gets or sets the collection of result messages associated with the query result.
### Methods


#### Constructor
Initializes a new instance of the 
 *See: T:OoBDev.System.ResponseModel.QueryResult`1*class. 


##### Parameters
* *items:* The collection of items in the result.




#### Constructor
Initializes a new instance of the 
 *See: T:OoBDev.System.ResponseModel.QueryResult`1*class by wrapping another 
 *See: T:OoBDev.System.ResponseModel.IQueryResult`1*. 


##### Parameters
* *toWrap:* The query result to wrap.




#### Constructor
Initializes a new instance of the 
 *See: T:OoBDev.System.ResponseModel.QueryResult`1*class with an empty collection of items. 


## Class: System.ResponseModel.ResultMessage
additional details about response 

### Properties

#### Level
importance level related to response
#### Message
Simple English message about issue.
#### MessageCode
unique code that may be used to assist in translating issue
#### Context
Property or path related to this message
#### MetaData
additional properties related to response

## Class: System.Security.Claims.ClaimsPrincipalExtensions
Extensions to manage 
 *See: T:System.Security.Claims.Claim*on 
 *See: T:System.Security.Claims.ClaimsPrincipal*
### Methods


#### GetAllClaims(System.Security.Claims.ClaimsPrincipal)
Iterate all 
 *See: T:System.Security.Claims.Claim*for provided 
 *See: T:System.Security.Claims.ClaimsPrincipal*

##### Parameters
* *principal:* 




##### Return value




#### GetClaimValues(System.Security.Claims.ClaimsPrincipal,System.String[])
Get for matched 
 *See: T:System.Security.Claims.Claim*on 
 *See: T:System.Security.Claims.ClaimsPrincipal*

##### Parameters
* *principal:* 
* *claims:* 




##### Return value




#### GetClaimValue(System.Security.Claims.ClaimsPrincipal,System.String[])
Get first matched 
 *See: T:System.Security.Claims.Claim*on 
 *See: T:System.Security.Claims.ClaimsPrincipal*

##### Parameters
* *principal:* 
* *claims:* 




##### Return value




## Class: System.Security.Claims.CommonClaims
Contains constants for common claims used in authentication. 

### Fields

#### UserId
Represents the claim for user ID.
#### Culture
Represents the claim for user culture.
#### ExtendedProperties
Represents the claim for extended properties.
#### ApplicationRight
Represents the claim for application rights.
#### ObjectId
Represents the claim for object ID.
#### ObjectIdentifier
Represents the claim for object identifier.
#### NameIdentifier
Represents the claim for name identifier.

## Class: System.Security.Cryptography.IHash
Simplified hash generator 

### Methods


#### GetHash(System.String)
cryptographic has for input value 


##### Parameters
* *value:* value to hash




##### Return value
hash input



## Class: System.Text.ISerializer
Interface to identify shared serialization process. 

### Properties

#### ContentType
Content type supported by this serializer
### Methods


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




## Class: System.Text.Json.Serialization.IBsonSerializer
interface to identify shared BSON serialization process. 


## Class: System.Text.Json.Serialization.IJsonSerializer
Represents an interface to identify a shared JSON serialization process. 

### Methods


#### AsPropertyName(System.String)
Converts the provided property name according to the configured property naming policy. 


##### Parameters
* *propertyName:* The original property name.




##### Return value
The converted property name.



## Class: System.Text.Json.Serialization.JsonStringEnumConverterEx`1
A custom JSON converter for serializing and deserializing enums as strings or numbers. 
The enum type to convert. 

### Methods


#### Read(System.Text.Json.Utf8JsonReader@,System.Type,System.Text.Json.JsonSerializerOptions)
Reads the JSON representation of the enum value and converts it to the specified enum type. 


##### Parameters
* *reader:* The JSON reader.
* *typeToConvert:* The type of the object to convert.
* *options:* The serializer options.




##### Return value
The deserialized enum value.



#### Write(System.Text.Json.Utf8JsonWriter,`0,System.Text.Json.JsonSerializerOptions)
Writes the JSON representation of the enum value. 


##### Parameters
* *writer:* The JSON writer.
* *value:* The enum value to serialize.
* *options:* The serializer options.




## Class: System.Text.Templating.FileType
Represents a file type, providing information about the file extension, content type, and whether it is a template type. 

### Properties

#### Extension
Gets or sets the file extension associated with the file type.
#### ContentType
Gets or sets the content type associated with the file type.
#### IsTemplateType
Gets or sets a value indicating whether the file type is a template type.

## Class: System.Text.Templating.IFileType
This interface allows objects that implement it to provide information about a specific file type, including its extension, content type, and whether it is considered a template type. 

### Properties

#### Extension
Gets the file extension.
#### ContentType
Gets the content type of the file.
#### IsTemplateType
Gets a value indicating whether the file type is considered a template type.
#### 
Gets a collection of file types.

## Class: System.Text.Templating.IFileTypeProvider
Provides a collection of file types. 

### Properties

#### Types
Gets a collection of file types.

## Class: System.Text.Templating.ITemplateContext
Represents the context for a template, including information about the template and target content types, source, and priority. 

### Properties

#### TemplateName
Gets the name of the template.
#### TemplateContentType
Gets the content type of the template.
#### TemplateFileExtension
Gets the file extension of the template.
#### TemplateSource
Gets the source of the template.
#### TemplateReference
Gets the reference identifier for the template.
#### OpenTemplate
Gets the function that opens the template as a stream.
#### TargetContentType
Gets the content type of the target.
#### TargetFileExtension
Gets the file extension of the target.
#### Priority
Gets the priority of the template, used to determine the order of template application.

## Class: System.Text.Templating.ITemplateEngine
Represents a template engine for generating content based on templates. 

### Methods


#### Get(System.String)
Gets the template context for the specified template name. 


##### Parameters
* *templateName:* The name of the template.




##### Return value
The template context for the specified template name or null if not found.



#### GetAll(System.String)
Gets all template contexts associated with the specified template name. 


##### Parameters
* *templateName:* The name of the template.




##### Return value
An enumeration of all template contexts associated with the specified template name.



#### ApplyAsync(System.String,System.Object,System.IO.Stream)
Applies the specified data to the template identified by its name and writes the result to the target stream asynchronously. 


##### Parameters
* *templateName:* The name of the template.
* *data:* The data to apply to the template.
* *target:* The stream where the result will be written.




##### Return value
A task representing the asynchronous operation, containing the template context for the applied template or null if not found.



#### ApplyAsync(OoBDev.System.Text.Templating.ITemplateContext,System.Object,System.IO.Stream)
Applies the specified data to the given template context and writes the result to the target stream asynchronously. 


##### Parameters
* *context:* The template context to apply.
* *data:* The data to apply to the template.
* *target:* The stream where the result will be written.




##### Return value
A task representing the asynchronous operation, indicating whether the application was successful.



#### ApplyAsync(System.String,System.Object)
Applies the specified data to the template identified by its name and returns the result as a string asynchronously. 


##### Parameters
* *templateName:* The name of the template.
* *data:* The data to apply to the template.




##### Return value
A task representing the asynchronous operation, containing the result as a string or null if not found.



#### ApplyAsync(OoBDev.System.Text.Templating.ITemplateContext,System.Object)
Applies the specified data to the given template context and returns the result as a string asynchronously. 


##### Parameters
* *context:* The template context to apply.
* *data:* The data to apply to the template.




##### Return value
A task representing the asynchronous operation, containing the result as a string or null if not found.



## Class: System.Text.Templating.ITemplateProvider
Represents a template provider that can apply templates based on a specified context. 

### Properties

#### SupportedContentTypes
Gets the collection of supported content types by the template provider.
### Methods


#### CanApply(OoBDev.System.Text.Templating.ITemplateContext)
Determines whether the template provider can apply a template based on the provided context. 


##### Parameters
* *context:* The template context.




##### Return value
true if the template provider can apply the template; otherwise, false.



#### ApplyAsync(OoBDev.System.Text.Templating.ITemplateContext,System.Object,System.IO.Stream)
Applies the template associated with the specified context, using the provided data, and writes the result to the target stream asynchronously. 


##### Parameters
* *context:* The template context.
* *data:* The data to apply to the template.
* *target:* The stream where the result will be written.




##### Return value
A task representing the asynchronous operation, indicating whether the application was successful.



## Class: System.Text.Templating.ITemplateSource
Represents a source of templates that can be used by a template engine. 

### Methods


#### Get(System.String)
Gets the template contexts associated with the specified template name. 


##### Parameters
* *templateName:* The name of the template to retrieve.




##### Return value
An enumerable collection of template contexts.



## Class: System.Text.Xml.Serialization.IXmlSerializer
interface to identify shared XML serialization process. 
