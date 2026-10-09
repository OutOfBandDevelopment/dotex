# OoBDev.Common.AspNetCore

This library provides a set of extension methods for configuring common ASP.NET Core middleware and extensions.

### Common.AspNetCore.ApplicationBuilderExtensions

This class provides extension methods for configuring common ASP.NET Core middleware.

### Methods

#### UseCommonAspNetCoreMiddleware(Microsoft.AspNetCore.Builder.IApplicationBuilder, OoBDev.Common.AspNetCore.MiddlewareExtensionBuilder)

Adds common ASP.NET Core middleware to the specified application builder.

##### Parameters

* `builder`: The instance.
* `middlewareBuilder`: The instance.

##### Return value

The updated instance.

### Common.AspNetCore.MiddlewareExtensionBuilder

This class represents a builder for configuring middleware extensions.

### Properties

#### HealthCheckPath

Gets or initializes the path for health checks.

### Common.AspNetCore.ServiceCollectionExtensions

This class provides extension methods for configuring common ASP.NET Core extensions in the `IServiceCollection`.

### Methods

#### TryCommonAspNetCoreExtensions(Microsoft.Extensions.DependencyInjection.IServiceCollection, Microsoft.Extensions.Configuration.IConfiguration, OoBDev.AspNetCore.Mvc.AspNetCoreExtensionBuilder, OoBDev.AspNetCore.JwtAuthentication.JwtExtensionBuilder)

Tries to add common ASP.NET Core extensions to the specified `IServiceCollection`.

##### Parameters

* `services`: The instance.
* `configuration`: The configuration containing settings for ASP.NET Core extensions.
* `aspNetBuilder`: Optional builder for configuring ASP.NET Core extensions. Default is null.
* `jwtBuilder`: Optional builder for configuring JWT extensions. Default is null.

##### Return value

The updated instance.

This library is designed to simplify the process of configuring common ASP.NET Core middleware and extensions in your ASP.NET Core applications.