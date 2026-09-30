# Authorization Approaches

<!-- nav -->
[↑ 05 — Industry Alternatives](./README.md) · [← HTTP API and Querying](./10-http-api.md) · [Security Approaches →](./12-security.md)
<!-- nav -->

## 26. Authorization model

**Today:** `[ApplicationRight]` filters check that the caller holds at least one required right from an application-right claim; `UserAuthorizationHandler` requires an identified user. The stated preference is RBAC with application rights declared per endpoint, roles translated to rights in middleware, and claims exchange as an alternative if tokens stay small ([authorization practices](../03-practices-and-conventions/11-authorization-practices.md)).

**Table 31 — Authorization models**

| Model | Pros | Cons |
|-------|------|------|
| RBAC with application rights (roles mapped to rights) | Simple to explain and audit; roles managed at the identity provider; rights stay stable in code | Role explosion in large organizations; no per-record decisions |
| Permission claims only (no roles) | No mapping layer | Token bloat; rights frozen until token expiry |
| Attribute-based access control (ABAC) | Decisions on user, resource and context attributes | Policy authoring and testing effort; performance cost |
| Relationship-based access control (Zanzibar style, for example OpenFGA, SpiceDB) | Per-record sharing and hierarchies | Extra service and data to keep in sync; different mental model |
| Policy engine (Open Policy Agent, Cedar) | Central, testable policy as code | Another runtime dependency; policy language to learn |
| ASP.NET Core policy-based authorization | Built in; composes requirements and handlers | Policies are code; needs discipline to keep rights as the vocabulary |

**Table 32 — Where rights get attached**

| Option | Pros | Cons |
|--------|------|------|
| Middleware translation from roles (preferred default) | Small tokens; mapping changes without reissuing tokens; per-application rights | Mapping lives in each application or a shared library; needs caching |
| Claims exchange at the STS | Mapping is central; applications only check claims | Token bloat with many rights; stale until expiry; STS must know each application's rights |
| Introspection or a rights endpoint called per request | Always current; tiny tokens | Latency and availability dependency; cache needed |
| Rights embedded by the identity provider | No extra component | Provider-specific; app rights leak into the provider |

Token size limits and library capabilities reflect general knowledge and should be re-checked before a decision.

**Verdict: Keep the preference.** RBAC with application rights, translated in injected middleware, with STS claims exchange as a config-gated alternative for short, stable rights lists. Consider ABAC or relationship-based checks only where per-record decisions are required, behind the same rights interface. Record the choice as an ADR.

---

<!-- nav -->
[↑ 05 — Industry Alternatives](./README.md) · [← HTTP API and Querying](./10-http-api.md) · [Security Approaches →](./12-security.md)
<!-- nav -->
