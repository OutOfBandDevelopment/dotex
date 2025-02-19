# OoBDev.Common.AspNetCore


## Class: Common.AspNetCore.ApplicationBuilderExtensions
Provides extension methods for configuring common ASP.NET Core middleware. 

### Methods


#### UseCommonAspNetCoreMiddleware(Microsoft.AspNetCore.Builder.IApplicationBuilder,OoBDev.Common.AspNetCore.MiddlewareExtensionBuilder)
Adds common ASP.NET Core middleware to the specified application builder. 


##### Parameters
* *builder:* The instance.
* *middlewareBuilder:* The instance.




##### Return value
The updated instance.



## Class: Common.AspNetCore.MiddlewareExtensionBuilder
Represents a builder for configuring middleware extensions. 

### Properties

#### HealthCheckPath
Gets or initializes the path for health checks.

## Class: Common.AspNetCore.ServiceCollectionExtensions
Provides extension methods for configuring common ASP.NET Core extensions in the 
 *See: T:Microsoft.Extensions.DependencyInjection.IServiceCollection*. 

### Methods


#### TryCommonAspNetCoreExtensions(Microsoft.Extensions.DependencyInjection.IServiceCollection,Microsoft.Extensions.Configuration.IConfiguration,OoBDev.AspNetCore.Mvc.AspNetCoreExtensionBuilder,OoBDev.AspNetCore.JwtAuthentication.JwtExtensionBuilder)
Tries to add common ASP.NET Core extensions to the specified 
 *See: T:Microsoft.Extensions.DependencyInjection.IServiceCollection*. 


##### Parameters
* *services:* The instance.
* *configuration:* The configuration containing settings for ASP.NET Core extensions.
* *aspNetBuilder:* Optional builder for configuring ASP.NET Core extensions. Default is null.
* *jwtBuilder:* Optional builder for configuring JWT extensions. Default is null.




##### Return value
The updated instance.

