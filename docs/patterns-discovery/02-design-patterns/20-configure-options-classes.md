# Pattern 20 — `IConfigureOptions<>` classes for third-party options

<!-- nav -->
[↑ 02 — Design Patterns (As Practiced in the Code)](./README.md) · [← Pattern 19 — ASP.NET cross-cutting: middleware + request features](./19-aspnet-middleware-request-features.md) · [Index →](./README.md)
<!-- nav -->

For OpenAPI, MVC and JWT, small classes implement `IConfigureOptions<OpenApiOptions>` (`ConfigureOpenApiOptions`, `ConfigureOAuthOpenApiOptions`) and `IConfigureOptions<MvcOptions>` (`AddMvcFilterOptions<T>`) so each concern configures the third-party options object independently, in DI, with injected dependencies, instead of one giant `AddOpenApi(o => …)` lambda. The classes are registered with `TryAddEnumerable`. See [OpenApiScalar](../../design/OpenApiScalar/README.md).

---

<!-- nav -->
[↑ 02 — Design Patterns (As Practiced in the Code)](./README.md) · [← Pattern 19 — ASP.NET cross-cutting: middleware + request features](./19-aspnet-middleware-request-features.md) · [Index →](./README.md)
<!-- nav -->
