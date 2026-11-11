# OoBDev.Common.Complete

Aggregator package that references every other `Common` aggregator (Abstractions, base, AspNetCore, Extensions, HardwareDevices, Hosting) and registers all of them in one call.

## Use

```csharp
services.TryAllCommonExtensions(configuration);            // namespace OoBDev.Common
app.UseAllCommonMiddleware();                              // optional MiddlewareExtensionBuilder
```

`TryAllCommonExtensions(configuration, systemBuilder, aspNetBuilder, jwtBuilder, identityBuilder, externalBuilder, hostingBuilder)` runs, in order, `TryCommonExtensions`, `TryCommonAspNetCoreExtensions`, `TryCommonExternalExtensions`, `TryCommonHardwareDeviceExtensions` and `TryCommonHosting`. Every builder is optional in release builds (null means defaults); in Debug builds all are required so a child builder is never forgotten. `UseAllCommonMiddleware` currently only calls `UseCommonAspNetCoreMiddleware` (health check endpoint).

Smaller entry points: [Common](../OoBDev.Common/README.Common.md), [Common.AspNetCore](../OoBDev.Common.AspNetCore/README.Common.AspNetCore.md), [Common.Extensions](../OoBDev.Common.Extensions/README.Common.Extensions.md), [Common.HardwareDevices](../OoBDev.Common.HardwareDevices/README.Common.HardwareDevices.md) and [Common.Hosting](../OoBDev.Common.Hosting/README.Common.Hosting.md).
