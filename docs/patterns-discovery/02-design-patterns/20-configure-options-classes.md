# Pattern 20 — `IConfigureOptions<>` classes for third-party options

<!-- nav -->
[↑ 02 — Design Patterns (As Practiced in the Code)](./README.md) · [← Pattern 19 — ASP.NET cross-cutting: middleware + request features](./19-aspnet-middleware-request-features.md) · [Index →](./README.md)
<!-- nav -->

For Swashbuckle, MVC and JWT, small classes implement `IConfigureOptions<SwaggerGenOptions>` (`AddOperationFilterOptions<T>`, `AddSchemaFilterOptions<T>`, `AddMvcFilterOptions<T>`, `ConfigureOAuthSwaggerGenOptions`) so each concern configures the third-party options object independently, in DI, with injected dependencies — instead of one giant `AddSwaggerGen(o => …)` lambda.

---

<!-- nav -->
[↑ 02 — Design Patterns (As Practiced in the Code)](./README.md) · [← Pattern 19 — ASP.NET cross-cutting: middleware + request features](./19-aspnet-middleware-request-features.md) · [Index →](./README.md)
<!-- nav -->
