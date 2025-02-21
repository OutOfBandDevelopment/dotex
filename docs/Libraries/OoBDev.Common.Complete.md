# OoBDev.Common.Complete


## Class: Common.ApplicationBuilderExtensions
Provides extension methods for configuring common middleware on the 
 *See: T:Microsoft.AspNetCore.Builder.IApplicationBuilder*. 

### Methods


#### UseAllCommonMiddleware(Microsoft.AspNetCore.Builder.IApplicationBuilder,OoBDev.Common.AspNetCore.MiddlewareExtensionBuilder)
Configures all common middleware on the specified 
 *See: T:Microsoft.AspNetCore.Builder.IApplicationBuilder*. 


##### Parameters
* *builder:* The instance.
* *middlewareBuilder:* The instance.




##### Return value
The updated instance.



## Class: Common.ServiceCollectionExtensions
Provides extension methods for configuring all common extensions in the 
 *See: T:Microsoft.Extensions.DependencyInjection.IServiceCollection*. 

### Methods


#### TryAllCommonExtensions(Microsoft.Extensions.DependencyInjection.IServiceCollection,Microsoft.Extensions.Configuration.IConfiguration,OoBDev.System.SystemExtensionBuilder,OoBDev.AspNetCore.Mvc.AspNetCoreExtensionBuilder,OoBDev.AspNetCore.JwtAuthentication.JwtExtensionBuilder,OoBDev.Common.Extensions.IdentityExtensionBuilder,OoBDev.Common.Extensions.ExternalExtensionBuilder,OoBDev.Common.Hosting.HostingBuilder)
Tries to add all common extensions to the specified 
 *See: T:Microsoft.Extensions.DependencyInjection.IServiceCollection*. 


##### Parameters
* *services:* The instance.
* *configuration:* The configuration containing settings for common extensions.
* *systemBuilder:* Optional builder for configuring system extensions. Default is null.
* *aspNetBuilder:* Optional builder for configuring ASP.NET Core extensions. Default is null.
* *jwtBuilder:* Optional builder for configuring JWT extensions. Default is null.
* *identityBuilder:* Optional builder for configuring identity extensions. Default is null.
* *externalBuilder:* Optional builder for configuring external extensions. Default is null.
* *hostingBuilder:* Optional builder for configuring hosting. Default is null.




##### Return value
The updated instance.

