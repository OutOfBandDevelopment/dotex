Here is the documentation for the source code in Markdown format:

**OoBDev.AspNetCore.JwtAuthentication**
=====================================

**Summary**
--------

This assembly contains methods for configuring JWT Bearer authentication for ASP.Net Core and SwaggerGen (OpenAPI3).

**Getting started**
-----------------

To use these extensions, first add a reference to the `OoBDev.AspNetCore.JwtAuthentication` package in your project. Then, add the following namespaces to your IOC registration code:

```csharp
using OoBDev.AspNetCore.JwtAuthentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
```

Next, configure your JWT Bearer authentication and SwaggerGen services in your `IServiceCollection` using the extension methods:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.TryAddJwtBearerServices(configuration);
}
```

where `configuration` is your `IConfiguration` instance.

These extensions will configure JWT Bearer authentication and add SwaggerGen services for OAuth2 using the configuration sections specified in the `JwtBearerOptions` and `OAuth2SwaggerOptions` classes.

**Configuration**
----------------

The JWT Bearer authentication configuration is specified in the `JwtBearerOptions` class. The OAuth2SwaggerOptions class specifies the configuration for SwaggerGen services for OAuth2.

The configuration for JWT Bearer authentication is specified in the `jwtBearerConfigurationSection` parameter of the TryAddJwtBearerAuthentication method. The configuration for SwaggerGen services for OAuth2 is specified in the `oAuth2SwaggerConfigurationSection` parameter of the TryAddJwtBearerSwaggerGen method.

By default, the `JwtBearerDefaults.AuthenticationScheme` and `OAuth2SwaggerOptions.DefaultScheme` values are used for the default authentication scheme and the default scheme for SwaggerGen services, respectively.

### Example

Here is an example of how to use the extension methods to configure JWT Bearer authentication and SwaggerGen services in an ASP.NET 8.0 project:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddControllers();

    services.TryAddJwtBearerServices(configuration);

    services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
    });
}
```

This code adds the JwtBearerAuthentication and SwaggerGen services to the `IServiceCollection` using the TryAddJwtBearerServices method. It also adds a Swagger documentation for an API with a version 1.

```plantuml
@startuml
interface IJwtBearerAuthentication
- authOptions: JwtBearerOptions
endinterface

interface IConfigureOptions<SwaggerUIOptions>
- options: ConfigureOAuthSwaggerUIOptions
endinterface

interface IConfigureOptions<SwaggerGenOptions>
- options: ConfigureOAuthSwaggerGenOptions
endinterface

class SwaggerGenOptions
    <<interface>>
endclass

class OAuth2SwaggerOptions
    <<interface>>
endclass

class JwtBearerOptions
    <<interface>>
endclass

class SwaggerUIOptions
    <<interface>>
endclass

class ConfigureOAuthSwaggerUIOptions
    <<class>>
endclass

class ConfigureOAuthSwaggerGenOptions
    <<class>>
endclass

class JwtExtensionBuilder
    <<record>>
endclass

OoBDev.AspNetCore.JwtAuthentication -> IJwtBearerAuthentication
OoBDev.AspNetCore.JwtAuthentication -> IConfigureOptions<SwaggerUIOptions>
OoBDev.AspNetCore.JwtAuthentication -> IConfigureOptions<SwaggerGenOptions>
IJwtBearerAuthentication -> JwtBearerOptions
IConfigureOptions<SwaggerUIOptions> -> ConfigureOAuthSwaggerUIOptions
IConfigureOptions<SwaggerGenOptions> -> ConfigureOAuthSwaggerGenOptions
SwaggerGenOptions -> OAuth2SwaggerOptions
JwtBearerOptions -> SwaggerUIOptions
@enduml
```

```plantuml
@startuml
interface IServiceCollection
- TryAddJwtBearerServices(config: IConfiguration): IServiceCollection
- TryAddJwtBearerAuthentication(config: IConfiguration, defaultScheme: string, configurationSection: string): IServiceCollection
- TryAddJwtBearerSwaggerGen(config: IConfiguration, configurationSection: string): IServiceCollection
endinterface

class ServiceCollectionExtensions
    <<class>>
endclass

ServiceCollectionExtensions -> IServiceCollection
IServiceCollection -> TryAddJwtBearerServices
IServiceCollection -> TryAddJwtBearerAuthentication
IServiceCollection -> TryAddJwtBearerSwaggerGen
@enduml
```