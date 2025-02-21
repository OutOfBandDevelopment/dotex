# OoBDev.System.Linq


## Class: System.Linq.Expressions.ExpressionExtensions
Extensions for System.Linq.Expressions.Expression 
 *See: T:System.Linq.Expressions.Expression*
### Methods


#### OrElse``1(System.Collections.Generic.IEnumerable{System.Linq.Expressions.Expression{System.Func{``0,System.Boolean}}})
Build an or'd expression chain 


##### Parameters
* *ors:* 




##### Return value




#### AndAlso``1(System.Collections.Generic.IEnumerable{System.Linq.Expressions.Expression{System.Func{``0,System.Boolean}}})
Build an and'd expression chain 


##### Parameters
* *ands:* 




##### Return value




## Class: System.Linq.Expressions.ExpressionTreeBuilder`1
Provides functionality for building and managing expression trees dynamically in the context of filtering data. 
The type of the data model. 

### Methods


#### Constructor
Provides functionality for building and managing expression trees dynamically in the context of filtering data. 


##### Parameters
* *logger:* Optional logger for logging messages.
* *capture:* Optional result message capturer.




#### GetPredicateExpression(System.String,OoBDev.System.Linq.Search.FilterParameter,System.StringComparison,System.Boolean)
Gets the predicate expression based on the provided parameters. 


##### Parameters
* *name:* The name of the property.
* *value:* The filter parameter value.
* *stringComparison:* The string comparison method.
* *isSearchTerm:* Flag indicating if the value is a search term.




##### Return value
The predicate expression or null if not found.



#### BuildExpression(System.Object,System.StringComparison,System.Boolean)
Builds an expression by combining multiple predicate expressions using OR logic. 


##### Parameters
* *queryParameter:* The query parameter for filtering.
* *stringComparison:* The string comparison method.
* *isSearchTerm:* Flag indicating if the value is a search term.




##### Return value
The combined predicate expression or null if not applicable.



#### PropertyExpressions
Builds the expressions for the properties in the data model. 


##### Return value
The dictionary containing property names and their corresponding expressions.



#### GetSearchablePropertyNames
Gets the searchable property names in the data model. 


##### Return value
The collection of searchable property names.



#### GetSortablePropertyNames
Gets the sortable property names in the data model. 


##### Return value
The collection of sortable property names.



#### GetFilterablePropertyNames
Gets the filterable property names in the data model. 


##### Return value
The collection of filterable property names.



#### DefaultSortOrder
Returns the default sort order based on attributes. 


##### Return value
The default sort order as a collection of column names and directions.



## Class: System.Linq.Expressions.SkipInstanceMethodOnNullExpressionVisitor
This visitor will modify expressions to add `x.Property != null` before instance method calls for query rewrite 

### Methods


#### VisitMethodCall(System.Linq.Expressions.MethodCallExpression)
If expression is an instance method then modify the expression to ensure a 


##### Parameters
* *node:* 




##### Return value




## Class: System.Linq.Expressions.SkipMemberOnNullExpressionVisitor
Represents a visitor for expression trees that skips member access when encountering null values. 

### Methods


#### VisitMethodCall(System.Linq.Expressions.MethodCallExpression)
Visits the method call expression node and modifies it to skip member access if the target object is null. 


##### Parameters
* *node:* The method call expression to visit.




##### Return value
The modified expression.



#### VisitLambda``1(System.Linq.Expressions.Expression{``0})
Visits the method call expression node and modifies it to skip member access if the target object is null. 


##### Parameters
* *node:* The method call expression to visit.




##### Return value
The modified expression.



## Class: System.Linq.Expressions.StringCasing
Specifies the string casing options for use in sorting and expression transformations. 

### Fields

#### Default
Use the default string casing (no transformation).
#### Upper
Transform strings to uppercase.
#### Lower
Transform strings to lowercase.

## Class: System.Linq.Expressions.StringComparisonReplacementExpressionVisitor
Expression visitor to replace string functions with the matching functions that end with a StringComparison parameter 

### Methods


#### Constructor
Expression visitor to replace string functions with the matching functions that end with a StringComparison parameter 


#### VisitBinary(System.Linq.Expressions.BinaryExpression)
Replace `string == string` with `string.Equals(string, StringComparison)` 


##### Parameters
* *node:* 




##### Return value




#### VisitMethodCall(System.Linq.Expressions.MethodCallExpression)
Replace `string.Xyz(string)` with `string.Xyz(string, StringComparison)` 


##### Parameters
* *input:* 




##### Return value




## Class: System.Linq.Expressions.StringOrderReplacementExpressionVisitor
A custom expression visitor that replaces string casing in OrderBy expressions within LINQ queries based on the specified 
 *See: T:OoBDev.System.Linq.Expressions.StringCasing*. 

### Methods


#### Constructor
A custom expression visitor that replaces string casing in OrderBy expressions within LINQ queries based on the specified 
 *See: T:OoBDev.System.Linq.Expressions.StringCasing*. 


##### Parameters
* *stringCasing:* Determines whether to convert strings to uppercase or lowercase in sorting operations.
* *logger:* Optional logger to capture debug information.




#### VisitMethodCall(System.Linq.Expressions.MethodCallExpression)
Visits method call expressions, rewriting them to adjust string casing for order-by operations in LINQ queries. 


##### Parameters
* *node:* The method call expression being visited.




##### Return value
The modified expression if applicable, otherwise the original expression.



## Class: System.Linq.Search.Operators
Expression builder operator options 

### Fields

#### Unknown
Unknown
#### EqualTo
Values are equal
#### InSet
Value is within set
#### LessThan
value is less than provided
#### LessThanOrEqualTo
value is less than or equal to provided
#### GreaterThan
value is greater than provided
#### GreaterThanOrEqualTo
value is less greater or equal to provided
#### NotEqualTo
value not equal to

## Class: System.Linq.Search.QueryBuilder
Provides a base class for building and executing queries based on search, filter, sort, and page criteria. 

### Fields

#### DefaultPageSize
Default page size when not defined on request
### Methods


#### Execute(System.Linq.IQueryable,OoBDev.System.Linq.Search.ISearchQuery,System.Collections.Generic.IEnumerable{OoBDev.System.Linq.Expressions.IPostBuildExpressionVisitor},Microsoft.Extensions.Logging.ILogger{OoBDev.System.Linq.Search.QueryBuilder},OoBDev.System.ResponseModel.ICaptureResultMessage)
Composes and executes a query build from ISearchTermQuery, IFilterQuery, ISortQuery, IPageQuery. 


##### Parameters
* *query:* The queryable data source.
* *searchQuery:* The search query parameters.
* *postBuildVisitors:* Optional post-build expression visitors.
* *logger:* Optional logger for logging messages.
* *capture:* Optional message capture for result messages.




##### Return value
The result of the query execution.



#### Execute``1(System.Linq.IQueryable{``0},OoBDev.System.Linq.Search.ISearchQuery,System.Collections.Generic.IEnumerable{OoBDev.System.Linq.Expressions.IPostBuildExpressionVisitor},Microsoft.Extensions.Logging.ILogger{OoBDev.System.Linq.Search.QueryBuilder},OoBDev.System.ResponseModel.ICaptureResultMessage)
Composes and executes a typed query build from ISearchTermQuery, IFilterQuery, ISortQuery, IPageQuery. 


##### Parameters
* *query:* The typed queryable data source.
* *searchQuery:* The search query parameters.
* *postBuildVisitors:* Optional post-build expression visitors.
* *logger:* Optional logger for logging messages.
* *capture:* Optional message capture for result messages.




##### Return value
The result of the typed query execution.



#### Execute``1(System.Linq.IQueryable{``0},OoBDev.System.Linq.Search.ISearchQuery,OoBDev.System.Linq.Expressions.IPostBuildExpressionVisitor,OoBDev.System.Linq.Expressions.IPostBuildExpressionVisitor[])
Composes and executes a typed query build from ISearchTermQuery, IFilterQuery, ISortQuery, IPageQuery. 


##### Parameters
* *query:* The typed queryable data source.
* *searchQuery:* The search query parameters.
* *postBuildVisitor:* A single post-build expression visitor.
* *postBuildVisitors:* Additional post-build expression visitors.




##### Return value
The result of the typed query execution.



#### Constructor
Provides a typed implementation for building and executing queries based on search, filter, sort, and page criteria. 


#### 
Composes and executes a query build from ISearchTermQuery, IFilterQuery, ISortQuery, IPageQuery. 


##### Parameters
* *query:* The queryable data source.
* *searchQuery:* The search query parameters.




##### Return value
The result of the query execution.



#### 
Composes and executes a query build from ISearchTermQuery, IFilterQuery, ISortQuery, IPageQuery. 


##### Parameters
* *query:* The queryable data source.
* *searchQuery:* The search query parameters.




##### Return value
The result of the query execution.



## Class: System.Linq.Search.QueryBuilder`1
Provides a typed implementation for building and executing queries based on search, filter, sort, and page criteria. 
The type of the model in the query. 

### Methods


#### Constructor
Provides a typed implementation for building and executing queries based on search, filter, sort, and page criteria. 


#### ExecuteBy(System.Linq.IQueryable{`0},OoBDev.System.Linq.Search.ISearchQuery)
Composes and executes a query build from ISearchTermQuery, IFilterQuery, ISortQuery, IPageQuery. 


##### Parameters
* *query:* The queryable data source.
* *searchQuery:* The search query parameters.




##### Return value
The result of the query execution.



#### ExecuteBy(System.Linq.IQueryable,OoBDev.System.Linq.Search.ISearchQuery)
Composes and executes a query build from ISearchTermQuery, IFilterQuery, ISortQuery, IPageQuery. 


##### Parameters
* *query:* The queryable data source.
* *searchQuery:* The search query parameters.




##### Return value
The result of the query execution.



## Class: System.Linq.Search.SortBuilder`1
Builder class for sorting IQueryable instances based on a provided sort query. 
The type of the model being sorted. 

### Methods


#### Constructor
Builder class for sorting IQueryable instances based on a provided sort query. 


##### Parameters
* *logger:* The logger used for logging sorting-related messages. If not provided, a default console logger will be used.
* *messages:* The result message capturer for publishing sorting-related messages.




#### SortBy(System.Linq.IQueryable{`0},OoBDev.System.Linq.Search.ISortQuery,OoBDev.System.Linq.Expressions.IExpressionTreeBuilder{`0},System.StringComparison)
Sorts an IQueryable instance based on the provided sort query. 


##### Parameters
* *query:* The IQueryable instance to be sorted.
* *searchRequest:* The sort query parameters.
* *treeBuilder:* The expression tree builder for building property expressions.
* *stringComparison:* The string comparison type to use when sorting string values.




##### Return value
An IOrderedQueryable instance representing the sorted result.



## Class: System.Linq.ServiceCollectionExtensions
Suggested IOC configurations 

### Methods


#### TryAddSearchQueryExtensions(Microsoft.Extensions.DependencyInjection.IServiceCollection,OoBDev.System.Linq.Expressions.StringCasing)
Add support for shared SearchQuery Extensions 


##### Parameters
* *services:* 
* *stringCasing:* Force ordinal casing for sort




##### Return value


