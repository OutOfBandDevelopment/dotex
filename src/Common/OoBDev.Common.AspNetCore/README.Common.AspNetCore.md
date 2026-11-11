# OoBDev.Common.AspNetCore

Aggregator for the ASP.NET Core projects of the framework (`OoBDev.AspNetCore.*` in `src/Framework`, tests excluded) with a registration and a middleware entry point.

## Use

```csharp
services.TryCommonAspNetCoreExtensions(configuration);     // aspNetBuilder and jwtBuilder optional in release builds
app.UseCommonAspNetCoreMiddleware();                       // or pass a MiddlewareExtensionBuilder
```

- `TryCommonAspNetCoreExtensions` calls `TryAddAspNetCoreExtensions(aspNetBuilder)` ([OoBDev.AspNetCore.Mvc](../../Framework/OoBDev.AspNetCore.Mvc/README.AspNetCore.Mvc.md)) and `TryAddJwtBearerServices(configuration, jwtBuilder)` ([JWT authentication](../../Framework/OoBDev.AspNetCore.JwtAuthentication/README.AspNetCore.JwtAuthentication.md)).
- `UseCommonAspNetCoreMiddleware` calls `UseAspNetCoreExtensionMiddleware` with the health check path from `MiddlewareExtensionBuilder.HealthCheckPath` (default `/health`; null leaves it off).

In Debug builds the builder parameters are required, in Release they default to null (the `#if DEBUG` pattern of the framework).
