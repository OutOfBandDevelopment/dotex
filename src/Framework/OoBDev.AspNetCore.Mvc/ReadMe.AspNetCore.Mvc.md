# OoBDev.AspNetCore.Mvc

## Summary

This code is part of an open-source project that provides extensions for ASP.NET Core. 
The extensions include authentication, authorization, and search query support. The code 
contains several functions and classes that are used to implement these extensions.

## Configuration

The configuration options for this code can be found in the ServiceCollectionExtensions 
class. The main configuration options are:

* `TryAddAspNetCoreExtensions` This function adds the necessary services to support all 
  ASP.NET Core extensions provided by this library. It takes several parameters, including 
  requireAuthenticatedByDefault, requireApplicationUserId, and authorizationPolicyBuilder.

* `AddRequireAuthenticatedUser` This function adds authentication requirements to the service 
  collection. It takes two parameters, requireApplicationUserId and authorizationPolicyBuilder.

* `TryAddCommonOpenApiExtensions` This function enables extensions for Swagger/OpenAPI.

* `TryAddAspNetCoreSearchQuery` This function enables extensions for shared search query 
  extensions.


## Example

Here is an example of how to configure the code

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.TryAddAspNetCoreExtensions(
        requireAuthenticatedByDefault: true,
        requireApplicationUserId: true,
        authorizationPolicyBuilder: policyBuilder =>
        {
            policyBuilder.RequireClaim("scope", "api1");
            policyBuilder.RequireClaim("scope", "api2");
        }
    );

    services.AddRequireAuthenticatedUser(
        requireApplicationUserId: true,
        authorizationPolicyBuilder: policyBuilder =>
        {
            policyBuilder.RequireClaim("role", "admin");
        }
    );

    services.TryAddCommonOpenApiExtensions();
    services.TryAddAspNetCoreSearchQuery();
}
```

In this example, authentication is required by default, and the application user ID is also 
required. Additionally, two authorization policies are added: one that requires 
authentication and two specific claims (scope), and another that requires authentication 
and a specific claim (role). Finally, the Swagger/OpenAPI and search query extensions are 
enabled.