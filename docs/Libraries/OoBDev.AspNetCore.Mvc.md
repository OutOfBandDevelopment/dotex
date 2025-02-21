# OoBDev.AspNetCore.Mvc


## Class: AspNetCore.Mvc.ApplicationBuilderExtensions
This is a set of extension configurations for ASP.Net Core 

### Methods


#### UseAspNetCoreExtensionMiddleware(Microsoft.AspNetCore.Builder.IApplicationBuilder,System.String)
Add custom middleware to ASP.Net to support these extensions 
 *See: T:OoBDev.AspNetCore.Mvc.Middleware.CorrelationInfoMiddleware* *See: T:OoBDev.AspNetCore.Mvc.Middleware.CultureInfoMiddleware* *See: T:OoBDev.AspNetCore.Mvc.Middleware.SearchQueryMiddleware*

##### Parameters
* *builder:* 
* *healthCheckPath:* 




##### Return value




## Class: AspNetCore.Mvc.AspNetCoreExtensionBuilder
Represents a builder for configuring ASP.NET Core extensions. 

### Properties

#### RequireAuthenticatedByDefault
Gets or sets a value indicating whether authentication is required by default. Set to true to require authentication by default; otherwise, set to false. The default value is .
#### RequireApplicationUserId
Gets or sets a value indicating whether an application user ID is required. Set to true to require an application user ID; otherwise, set to false. The default value is .
#### AuthorizationPolicyBuilder
Gets or sets the delegate for configuring an . Set to a delegate that configures an . The default value is null.

## Class: AspNetCore.Mvc.Authorization.UserAuthorizationHandler
Handles user authorization based on specified requirements. 

Initializes a new instance of the class.
### Methods


#### Constructor
Initializes a new instance of the class.
Handles user authorization based on specified requirements. 


##### Parameters
* *logger:* The logger.




#### HandleRequirementAsync(Microsoft.AspNetCore.Authorization.AuthorizationHandlerContext,OoBDev.AspNetCore.Mvc.Authorization.UserAuthorizationRequirement)
Handles the user authorization requirement asynchronously. 


##### Parameters
* *context:* The authorization context.
* *requirement:* The user authorization requirement.




##### Return value
A task representing the asynchronous operation.



## Class: AspNetCore.Mvc.Authorization.UserAuthorizationRequirement
Represents an authorization requirement for user-specific authorization. 

### Fields

#### RequireAuthenticatedByDefault
The default value indicating whether authentication is required for authorization.
### Properties

#### RequireApplicationUserId
Gets a value indicating whether the application user ID is required for authorization.
### Methods


#### Constructor
Initializes a new instance of the 
 *See: T:OoBDev.AspNetCore.Mvc.Authorization.UserAuthorizationRequirement*class. 


##### Parameters
* *requireApplicationUserId:* Specifies whether the application user ID is required for authorization.




## Class: AspNetCore.Mvc.Diagnostics.HealthChecks.HealthCheckOptionsFactory
Factory class for creating health check options. 

### Methods


#### Create
Creates and configures a new instance of 
 *See: T:Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions*. 


##### Return value
The configured instance.



## Class: AspNetCore.Mvc.Filters.ApplicationPermissionsApiFilter
operation filter to extend swagger to include application rights 

### Methods


#### Apply(Microsoft.OpenApi.Models.OpenApiOperation,Swashbuckle.AspNetCore.SwaggerGen.OperationFilterContext)
Applies the operation filter to include application rights in Swagger documentation. 


##### Parameters
* *operation:* The OpenApiOperation to be modified.
* *context:* The OperationFilterContext providing information about the operation.




## Class: AspNetCore.Mvc.Filters.FormFileOperationFilter
Implementation of 
 *See: T:Swashbuckle.AspNetCore.SwaggerGen.IOperationFilter*for handling file upload in Swagger UI. 

### Methods


#### Apply(Microsoft.OpenApi.Models.OpenApiOperation,Swashbuckle.AspNetCore.SwaggerGen.OperationFilterContext)
Applies the file upload operation filter to modify the Swagger UI documentation. 


##### Parameters
* *operation:* The OpenApiOperation representing the Swagger operation.
* *context:* The OperationFilterContext providing information about the operation.




## Class: AspNetCore.Mvc.Filters.SearchQueryOperationFilter
Search Query Operation filter extends Swagger/OpenAPI to provide details on IQueryable{T} endpoints. 

### Methods


#### Constructor
Search Query Operation filter extends Swagger/OpenAPI to provide details on IQueryable{T} endpoints. 


#### Apply(Microsoft.OpenApi.Models.OpenApiOperation,Swashbuckle.AspNetCore.SwaggerGen.OperationFilterContext)
Applies the Search Query Operation filter to Swagger/OpenAPI. 


##### Parameters
* *operation:* The OpenApiOperation to apply the filter to.
* *context:* The OperationFilterContext containing information about the operation.




## Class: AspNetCore.Mvc.Filters.SearchQueryResultFilter
Search Query Results filter is an extension for ASP.Net Core to enable a common pattern to query data endpoints with from Controller Actions. This detects actions that return IQueryable{TModel} and intercepts the web request to complete the query based on user requested inputs. 
 *See: T:OoBDev.System.Linq.Search.SearchQuery`1* *See: T:OoBDev.System.Linq.Search.IQueryBuilder`1* *See: T:System.Linq.IQueryable`1* *See: T:OoBDev.System.ResponseModel.IQueryResult`1* *See: T:OoBDev.System.ResponseModel.IPagedQueryResult`1*
### Methods


#### Constructor
Search Query Results filter is an extension for ASP.Net Core to enable a common pattern to query data endpoints with from Controller Actions. This detects actions that return IQueryable{TModel} and intercepts the web request to complete the query based on user requested inputs. 
 *See: T:OoBDev.System.Linq.Search.SearchQuery`1* *See: T:OoBDev.System.Linq.Search.IQueryBuilder`1* *See: T:System.Linq.IQueryable`1* *See: T:OoBDev.System.ResponseModel.IQueryResult`1* *See: T:OoBDev.System.ResponseModel.IPagedQueryResult`1*

#### OnResultExecuted(Microsoft.AspNetCore.Mvc.Filters.ResultExecutedContext)
Called after the action result executes. 


##### Parameters
* *context:* The .




#### OnResultExecuting(Microsoft.AspNetCore.Mvc.Filters.ResultExecutingContext)
Called before the action result executes. 


##### Parameters
* *context:* The .




## Class: AspNetCore.Mvc.Middleware.CorrelationInfoMiddleware
Middleware for handling correlation information in HTTP requests and responses. 

### Methods


#### Constructor
Initializes a new instance of the 
 *See: T:OoBDev.AspNetCore.Mvc.Middleware.CorrelationInfoMiddleware*class. 


##### Parameters
* *next:* The delegate representing the next middleware in the pipeline.




#### Invoke(Microsoft.AspNetCore.Http.HttpContext,Microsoft.Extensions.Logging.ILogger{OoBDev.AspNetCore.Mvc.Middleware.CorrelationInfoMiddleware},OoBDev.System.Accessors.IAccessor{OoBDev.System.Net.Http.CorrelationInfo})
Invokes the middleware to handle correlation information in the HTTP context. 


##### Parameters
* *context:* The HTTP context.
* *logger:* The logger for logging correlation information.
* *correlationAccessor:* The accessor for managing correlation information.




##### Return value
A task representing the asynchronous operation.



## Class: AspNetCore.Mvc.Middleware.CultureInfoMiddleware
Custom middleware to enable detection of language/culture from HTTP request header as well as assignment for response header 

### Methods


#### Constructor
Custom middleware to enable detection of language/culture from HTTP request header as well as assignment for response header 


##### Parameters
* *next:* The next middleware in the pipeline.




#### Invoke(Microsoft.AspNetCore.Http.HttpContext,Microsoft.Extensions.Logging.ILogger{OoBDev.AspNetCore.Mvc.Middleware.CultureInfoMiddleware},OoBDev.System.Accessors.IAccessor{System.Globalization.CultureInfo})
Invokes the middleware to process the request and response. 


##### Parameters
* *context:* The HTTP context.
* *logger:* The logger.
* *cultureInfo:* The accessor for .




##### Return value
A task representing the asynchronous operation.



## Class: AspNetCore.Mvc.Middleware.SearchQueryMiddleware
ASP.Net MVC Middleware to enable IQueryable{T} responses from Controller Actions 

### Methods


#### Constructor
ASP.Net MVC Middleware to enable IQueryable{T} responses from Controller Actions 


##### Parameters
* *next:* The next middleware in the pipeline.
* *log:* The logger for logging information.
* *searchQuery:* The accessor for the search query.
* *builder:* The builder for search model.




#### InvokeAsync(Microsoft.AspNetCore.Http.HttpContext)
Invokes the middleware to handle the request. 


##### Parameters
* *context:* The HTTP context.




##### Return value
A task representing the asynchronous operation.



## Class: AspNetCore.Mvc.OpenApi.ApiPermissionsExtension
Declare permissions required for application endpoint 

### Properties

#### AllowAnonymous
End point allows unauthenticated requests
#### Rights
end point requires at least one of these permissions
### Methods


#### Constructor
Declare permissions required for application endpoint 


##### Parameters
* *allowAnonymous:* End point allows unauthenticated requests
* *rights:* end point requires at least one of these permissions




#### Write(Microsoft.OpenApi.Writers.IOpenApiWriter,Microsoft.OpenApi.OpenApiSpecVersion)
generate a property to provide the required permissions 


##### Parameters
* *writer:* 
* *specVersion:* 




## Class: AspNetCore.Mvc.Providers.SearchQuery.ISearchModelBuilder
Builds search models based on the incoming HTTP context. 

### Methods


#### GetModelAsync``1(Microsoft.AspNetCore.Http.HttpContext,System.String)
Gets the search model asynchronously from the provided HTTP context. 


##### Parameters
* *context:* The HTTP context containing the model data.
* *modelName:* The name of the model in the context.




##### Return value
A tuple indicating whether the search is successful and the retrieved model.



## Class: AspNetCore.Mvc.Providers.SearchQuery.ISearchModelMapper
Maps search-related model information from the HTTP context. 

### Methods


#### GetControllerActionDescriptor(Microsoft.AspNetCore.Http.HttpContext)
Retrieves the controller action descriptor from the provided HTTP context. 


##### Parameters
* *context:* The HTTP context.




##### Return value
The controller action descriptor if found; otherwise, null.



#### GetActionContext(Microsoft.AspNetCore.Http.HttpContext,Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor)
Creates an action context from the provided HTTP context and controller action descriptor. 


##### Parameters
* *context:* The HTTP context.
* *descriptor:* The controller action descriptor.




##### Return value
The created action context if both parameters are valid; otherwise, null.



#### GetControllerContext(Microsoft.AspNetCore.Mvc.ActionContext)
Creates a controller context from the provided action context. 


##### Parameters
* *context:* The action context.




##### Return value
The created controller context if the action context is valid; otherwise, null.



#### GetRequestType(Microsoft.AspNetCore.Http.HttpContext)
Determines the type of request based on the provided HTTP context. 


##### Parameters
* *context:* The HTTP context.




##### Return value
The type of request.



#### GetBindingType(OoBDev.AspNetCore.Mvc.Providers.SearchQuery.RequestType)
Retrieves the binding type metadata based on the request type. 


##### Parameters
* *requestType:* The type of request.




##### Return value
The binding type metadata.



#### GetValueProviderAsync(Microsoft.AspNetCore.Http.HttpContext,Microsoft.AspNetCore.Mvc.ControllerContext,OoBDev.AspNetCore.Mvc.Providers.SearchQuery.RequestType)
Retrieves the value provider asynchronously based on the request type. 


##### Parameters
* *context:* The HTTP context.
* *controllerContext:* The controller context.
* *requestType:* The type of request.




##### Return value
The value provider.



## Class: AspNetCore.Mvc.Providers.SearchQuery.RequestType
Represents the type of HTTP request. 

### Fields

#### Query
Indicates a request with JSON data.
#### Json
Indicates a request with JSON data.
#### Form
Indicates a request with form data.

## Class: AspNetCore.Mvc.Providers.SearchQuery.SearchModelBuilder
Builds search models based on the incoming HTTP context. 

### Methods


#### Constructor
Initializes a new instance of the 
 *See: T:OoBDev.AspNetCore.Mvc.Providers.SearchQuery.SearchModelBuilder*class. 


##### Parameters
* *mapper:* The search model mapper.




#### GetModelAsync``1(Microsoft.AspNetCore.Http.HttpContext,System.String)
Gets the search model asynchronously from the provided HTTP context. 


##### Parameters
* *context:* The HTTP context containing the model data.
* *modelName:* The name of the model in the context.




##### Return value
A tuple indicating whether the search is successful and the retrieved model.



## Class: AspNetCore.Mvc.Providers.SearchQuery.SearchModelMapper
Maps search-related model information from the HTTP context. 

### Methods


#### GetControllerActionDescriptor(Microsoft.AspNetCore.Http.HttpContext)
Retrieves the controller action descriptor from the provided HTTP context. 


##### Parameters
* *context:* The HTTP context.




##### Return value
The controller action descriptor if found; otherwise, null.



#### GetActionContext(Microsoft.AspNetCore.Http.HttpContext,Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor)
Creates an action context from the provided HTTP context and controller action descriptor. 


##### Parameters
* *context:* The HTTP context.
* *descriptor:* The controller action descriptor.




##### Return value
The created action context if both parameters are valid; otherwise, null.



#### GetControllerContext(Microsoft.AspNetCore.Mvc.ActionContext)
Creates a controller context from the provided action context. 


##### Parameters
* *context:* The action context.




##### Return value
The created controller context if the action context is valid; otherwise, null.



#### GetRequestType(Microsoft.AspNetCore.Http.HttpContext)
Determines the type of request based on the provided HTTP context. 


##### Parameters
* *context:* The HTTP context.




##### Return value
The type of request.



#### GetBindingType(OoBDev.AspNetCore.Mvc.Providers.SearchQuery.RequestType)
Retrieves the binding type metadata based on the request type. 


##### Parameters
* *requestType:* The type of request.




##### Return value
The binding type metadata.



#### GetValueProviderAsync(Microsoft.AspNetCore.Http.HttpContext,Microsoft.AspNetCore.Mvc.ControllerContext,OoBDev.AspNetCore.Mvc.Providers.SearchQuery.RequestType)
Retrieves the value provider asynchronously based on the request type. 


##### Parameters
* *context:* The HTTP context.
* *controllerContext:* The controller context.
* *requestType:* The type of request.




##### Return value
The value provider.



## Class: AspNetCore.Mvc.ServiceCollectionExtensions
Extension methods for configuring ASP.Net Core extensions and related services. 

### Methods


#### TryAddAspNetCoreExtensions(Microsoft.Extensions.DependencyInjection.IServiceCollection,OoBDev.AspNetCore.Mvc.AspNetCoreExtensionBuilder)
Adds IOC configurations to support all ASP.Net Core extensions provided by this library. 


##### Parameters
* *services:* The service collection to which ASP.Net Core extensions should be added.
* *builder:* Indicates whether authentication is required by default.




##### Return value
The modified service collection.



#### AddRequireAuthenticatedUser(Microsoft.Extensions.DependencyInjection.IServiceCollection,System.Boolean,System.Action{Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder})
Adds authentication requirements to the service collection. 


##### Parameters
* *services:* The service collection to which authentication requirements should be added.
* *requireApplicationUserId:* Indicates whether the application user ID is required.
* *authorizationPolicyBuilder:* Action to configure the authorization policy builder.




##### Return value
The modified service collection.



#### TryAddCommonOpenApiExtensions(Microsoft.Extensions.DependencyInjection.IServiceCollection)
Enables extensions for Swagger/OpenAPI (included in AddAspNetCoreExtensions). 


##### Parameters
* *services:* The service collection to which Swagger/OpenAPI extensions should be added.




##### Return value
The modified service collection.



#### TryAddAspNetCoreSearchQuery(Microsoft.Extensions.DependencyInjection.IServiceCollection)
Enables extensions for shared Search Query extensions (included in AddAspNetCoreExtensions). 


##### Parameters
* *services:* The service collection to which Search Query extensions should be added.




##### Return value
The modified service collection.



## Class: AspNetCore.Mvc.SwaggerGen.AdditionalSwaggerGenEndpointsOptions
SwaggerGen extensions to enable presenting permissions, application versions and XMLDocs 

### Methods


#### Constructor
SwaggerGen extensions to enable presenting permissions, application versions and XMLDocs 


#### Configure(Swashbuckle.AspNetCore.SwaggerGen.SwaggerGenOptions)
Configures SwaggerGen options with additional features such as presenting permissions, application versions, and XMLDocs. 


##### Parameters
* *options:* The SwaggerGen options to be configured.




## Class: AspNetCore.Mvc.SwaggerGen.AdditionalSwaggerUIEndpointsOptions
SwaggerUI Extension to enable grouping controller/actions by assembly 

### Methods


#### Constructor
SwaggerUI Extension to enable grouping controller/actions by assembly 


#### Configure(Swashbuckle.AspNetCore.SwaggerUI.SwaggerUIOptions)
Configures SwaggerUI options to enable grouping of controller/actions by assembly. 


##### Parameters
* *options:* The SwaggerUI options to be configured.




## Class: AspNetCore.Mvc.SwaggerGen.AddMvcFilterOptions`1
register additional ASP.Net MVC Filters 

### Methods


#### Configure(Microsoft.AspNetCore.Mvc.MvcOptions)
Configures MVC options to add the specified filter. 


##### Parameters
* *options:* The MVC options to be configured.




## Class: AspNetCore.Mvc.SwaggerGen.AddOperationFilterOptions`1
Register additional IOperationFilters 

### Methods


#### Configure(Swashbuckle.AspNetCore.SwaggerGen.SwaggerGenOptions)
Configures SwaggerGen options to add the specified operation filter. 


##### Parameters
* *options:* The SwaggerGen options to be configured.




## Class: AspNetCore.Mvc.SwaggerGen.AddSchemaFilterOptions`1
Register additional IOperationFilters 

### Methods


#### Configure(Swashbuckle.AspNetCore.SwaggerGen.SwaggerGenOptions)
Register additional IOperationFilters 


##### Parameters
* *options:* 




## Class: AspNetCore.Mvc.SwaggerGen.ApiNamespaceControllerModelConvention
SwaggerGen extension to configure controller group as the related assembly name 

### Methods


#### Apply(Microsoft.AspNetCore.Mvc.ApplicationModels.ControllerModel)
Applies the convention to the specified controller model. 


##### Parameters
* *controller:* The controller model to apply the convention to.




## Class: AspNetCore.Mvc.SwaggerGen.HealthChecksDocumentFilter
Represents a document filter for health checks in the OpenAPI document. 

### Fields

#### HealthCheckEndpoint
The endpoint for health check.
### Methods


#### Apply(Microsoft.OpenApi.Models.OpenApiDocument,Swashbuckle.AspNetCore.SwaggerGen.DocumentFilterContext)
Applies the health check filter to the OpenAPI document. 


##### Parameters
* *openApiDocument:* The OpenAPI document to which the filter is applied.
* *context:* The context for the document filter.




## Class: AspNetCore.Mvc.SwaggerGen.HealthCheckSwaggerGenEndpointOptions
Represents configuration options for SwaggerGen related to health check endpoints. 

### Methods


#### Configure(Swashbuckle.AspNetCore.SwaggerGen.SwaggerGenOptions)
Configures SwaggerGen options to use the 
 *See: T:OoBDev.AspNetCore.Mvc.SwaggerGen.HealthChecksDocumentFilter*for filtering documents. 


##### Parameters
* *options:* The SwaggerGen options to configure.


