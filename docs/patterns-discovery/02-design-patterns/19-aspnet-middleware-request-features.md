# Pattern 19 — ASP.NET cross-cutting: middleware + request features

<!-- nav -->
[↑ 02 — Design Patterns (As Practiced in the Code)](./README.md) · [← Pattern 18 — Replace-to-override in host layers](./18-replace-to-override-in-host-layers.md) · [Pattern 20 — `IConfigureOptions<>` classes for third-party options →](./20-configure-options-classes.md)
<!-- nav -->

`CorrelationInfoMiddleware`, `CultureInfoMiddleware`, `SearchQueryMiddleware` populate per-request state; `IHttpPrepareRequestFeature` (multiple registrations) lets outgoing `HttpClient` calls copy correlation headers; `IPrincipal`/`IIdentity`/`ClaimsPrincipal` are exposed as DI services derived from `IHttpContextAccessor`; authorization uses a custom `UserAuthorizationHandler` that maps bearer-token principals to internal users plus `[ApplicationRight]` filters. `Accessor<T>`/`AddAccessor<CultureInfo>()` provide simple ambient values.

---

<!-- nav -->
[↑ 02 — Design Patterns (As Practiced in the Code)](./README.md) · [← Pattern 18 — Replace-to-override in host layers](./18-replace-to-override-in-host-layers.md) · [Pattern 20 — `IConfigureOptions<>` classes for third-party options →](./20-configure-options-classes.md)
<!-- nav -->
