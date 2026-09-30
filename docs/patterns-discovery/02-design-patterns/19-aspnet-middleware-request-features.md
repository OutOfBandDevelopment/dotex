# Pattern 19 — ASP.NET cross-cutting: middleware + request features

[↑ Design Patterns](./README.md) · [← 18. Replace-to-override in host layers](./18-replace-to-override-in-host-layers.md) · [20. `IConfigureOptions<>` classes for third-party options →](./20-configure-options-classes.md)

`CorrelationInfoMiddleware`, `CultureInfoMiddleware`, `SearchQueryMiddleware` populate per-request state; `IHttpPrepareRequestFeature` (multiple registrations) lets outgoing `HttpClient` calls copy correlation headers; `IPrincipal`/`IIdentity`/`ClaimsPrincipal` are exposed as DI services derived from `IHttpContextAccessor`; authorization uses a custom `UserAuthorizationHandler` that maps bearer-token principals to internal users plus `[ApplicationRight]` filters. `Accessor<T>`/`AddAccessor<CultureInfo>()` provide simple ambient values.

---

[↑ Design Patterns](./README.md) · [← 18. Replace-to-override in host layers](./18-replace-to-override-in-host-layers.md) · [20. `IConfigureOptions<>` classes for third-party options →](./20-configure-options-classes.md)
