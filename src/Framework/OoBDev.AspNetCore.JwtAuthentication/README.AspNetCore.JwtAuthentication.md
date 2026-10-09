# OoBDev.AspNetCore.JwtAuthentication

## Summary

This assembly contains methods for configuring JWT Bearer authentication for ASP.Net Core
and the OAuth2 sign-in for the OpenAPI documents and the Scalar API reference.

## Getting started

To use these extensions, first add a reference to the `OoBDev.AspNetCore.JwtAuthentication`
package in your project. Then, add the following namespaces to your IOC registation code:

```csharp
using OoBDev.AspNetCore.JwtAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
```

Next, configure your JWT Bearer authentication and OpenAPI services in your
IServiceCollection using the extension methods:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.TryAddJwtBearerServices(configuration);
}
```

where `configuration` is your `IConfiguration` instance.

These extensions configure JWT Bearer authentication and add the OpenAPI pieces for OAuth2
using the configuration sections specified in the JwtBearerOptions and OAuth2OpenApiOptions classes:

- `OAuthDocumentTransformer` declares the `oauth2` authorization code security scheme in every OpenAPI document.
- `OAuthApiReferenceConfigurator` configures the Scalar API reference to sign in with the authorization code flow and PKCE,
  using the JwtBearer `Audience` as the client id.

## Configuration

The JWT Bearer authentication configuration is specified in the JwtBearerOptions class. The
OAuth2OpenApiOptions class (`UserReadApiClaim`, `AuthorizationUrl`, `TokenUrl`) specifies the OAuth2 sign-in for the documents.

The configuration for JWT Bearer authentication is specified in the jwtBearerConfigurationSection
parameter of the TryAddJwtBearerAuthentication method. The configuration for the OpenAPI OAuth2 sign-in
is specified in the configurationSection parameter of the
TryAddJwtBearerOpenApi method (default section `OAuth2OpenApiOptions`).

## Example

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.TryAddAspNetCoreExtensions(configuration);   // OpenAPI documents and transformers
    services.TryAddJwtBearerServices(configuration);      // JWT bearer and the OAuth2 scheme
}

// pipeline
app.MapControllers();
app.MapApiReference();   // /openapi/{document}.json and /scalar/{document}
```

Each assembly that contains controllers gets its own document (`/openapi/{AssemblyName}.json`) and
the `all` document contains every endpoint.
