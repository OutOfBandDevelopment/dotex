Here is the documentation for the provided source code in Markdown format:

**OoBDev.System.Linq**

The OoBDev.System.Linq library provides a collection of LINQ/Query extensions that simplify query expressions to a simple, serializable model.

**Design Goals and Use Cases**

* Create a query model that allows for an easy-to-follow and reusable pattern
* Data selection patterns
* Multiple field sorting with the ability to select direction and priority order
* Data paging with the ability to change page size or disable paging to return all rows
* Make it easy to implement for developers

**Technical Requirements**

* Define Query Request Model
* Define Paged Result Models
* Define Patterns for Filter
* Define Wild card expressions
* Define Patterns for Sort Order
* Define Patterns for Paging Request
* Extend ASP.Net Middleware to intercept and extend `IQueryable<T>` returns from Controller/Actions
* Extend OpenAPI to present notes on available fields for request and response
* Create Expression Tree based framework to convert request models into LINQ queries
* HTTP Request type and formatting is detected from the related `Method` and `Content-Type`

**Architecture**

### HTTP Data Query

```plantuml
title Query Process

User -> WebApi: Send Query Request
activate WebApi
  WebApi -> Middleware: Detect Action Return
  activate Middleware
    alt return is IQueryable
      Middleware -> Middleware : Store Request
      Middleware -> Action : Get Query Return
      activate Action
      return IQueryable
      Middleware -> QueryBuilder : Compose Query
      activate QueryBuilder
      return PagedResult      
    end
  return PagedResult
return PagedResult
```

**API Template**

### Query

**Models**

### Request

* `CurrentPage`: the current page number
* `PageSize`: the number of records per page
* `ExcludePageCount`: a boolean value indicating whether to exclude page count from the result set
* `SearchTerm`: the search term to use in the query
* `Filter`: a dictionary of filter parameters
* `OrderBy`: a dictionary of order by parameters

### Response

* `Rows`: an array of data rows

**Technical Considerations**

### Security

Authentication and authorization follow standard ASP.Net MVC configurations.

### Performance

To improve performance, queries will be composed by LINQ to the related query provider allowing for data server processing including.

**Examples**

### Data Model

Here is an example data model that has been annotated with some of the attributes:
```csharp
[Searchable(FirstNameLastName)]
[Searchable(LastNameFirstName)]
[Filterable(Module)]
[Filterable(UserStatus)]
[SearchTermDefault(SearchTermDefaults.StartsWith)]
public class User
{
    public const string FirstNameLastName = nameof(FirstNameLastName);
    public const string LastNameFirstName = nameof(LastNameFirstName);
    public const string Module = nameof(Module);
    public const string UserStatus = nameof(UserStatus);

    public string? UserId { get; set; }
    public string? UserName { get; set; }

    [Searchable]
    [DefaultSort(priority: 2, order: OrderDirections.Ascending)]
    public string? EmailAddress { get; set; }

    [Searchable]
    [DefaultSort(priority: 1, order: OrderDirections.Ascending)]
    public string? FirstName { get; set; }

    [Searchable]
    [DefaultSort(priority: 0, order: OrderDirections.Ascending)]
    public string? LastName { get; set; }

    public bool Active { get; set; }

    public List<UserModule>? UserModules { get; set; }

    public DateTimeOffset? CreatedOn { get; set; }

    public static Expression<Func<User, object>>? PropertyMap(string key) =>
        key switch
        {
            FirstNameLastName => e => e.FirstName + " " + e.LastName,
            LastNameFirstName => e => e.LastName + " " + e.FirstName,
            _ => null
        };

    public static Expression<Func<User, bool>>? PredicateMap(string key, object value) =>
        key switch
        {
            Module => e => e.UserModules.Any(um => um.Code.Equals(value)),
            UserStatus => e => value.Equals("-1") || e.Active == value.Equals("1"),
            _ => null
        };
}
```
**Maps**

* `PropertyMap`: a static method that returns an expression that maps a property to a composite property
* `PredicateMap`: a static method that returns an expression that maps a predicate to a filtered result

**Post Build Visitors**

* `IPostBuildExpressionVisitor`: an interface that allows for modifying the expression tree before the query is executed
* `StringComparisonReplacementExpressionVisitor`: a visitor that replaces string functions with their `StringCompare` equivalents

**Search Query Intercept**

* `ISearchQueryIntercept`: an interface that allows for intercepting and modifying the SearchQuery
* `UnquoteAttribute`: an attribute that removes quotes from the beginning and end of