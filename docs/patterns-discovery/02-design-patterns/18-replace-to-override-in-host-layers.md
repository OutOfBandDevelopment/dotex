# Pattern 18 — Replace-to-override in host layers

[↑ Design Patterns](./README.md) · [← 17. Attribute + mapper data access](./17-attribute-mapper-data-access.md) · [19. ASP.NET cross-cutting: middleware + request features →](./19-aspnet-middleware-request-features.md)

Framework registers a safe default; the ASP.NET layer **replaces** it when a richer implementation is available:

```csharp
services.TryAddTransient<ICurrentUserAccessor, EnvironmentUserAccessor>();             // System
services.Replace(ServiceDescriptor.Describe(typeof(ICurrentUserAccessor),
    sp => sp.GetRequiredKeyedService<ICurrentUserAccessor>("HTTP"), ServiceLifetime.Transient)); // ASP.NET
```

Both are also registered keyed (`"Environment"`, `"HTTP"`) so either can still be requested explicitly.

---

[↑ Design Patterns](./README.md) · [← 17. Attribute + mapper data access](./17-attribute-mapper-data-access.md) · [19. ASP.NET cross-cutting: middleware + request features →](./19-aspnet-middleware-request-features.md)
