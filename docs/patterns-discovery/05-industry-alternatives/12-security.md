# Security Approaches

<!-- nav -->
[↑ 05 — Industry Alternatives](./README.md) · [← Authorization Approaches](./11-authorization.md) · [Observability Approaches →](./13-observability.md)
<!-- nav -->

## 27. Secrets, abuse control and supply chain

**Today:** secrets come through `IConfiguration` (environment variables and files); HTTPS redirection appears in the example API; SBOM output is kept under `docs/sbom`. There is no shared CORS, rate limiting, vault or audit code ([security practices](../03-practices-and-conventions/12-security-practices.md)).

**Table 33 — Secret management options**

| Option | Pros | Cons |
|--------|------|------|
| Environment variables and user secrets through `IConfiguration` (current) | No dependency; works everywhere; fits containers | No rotation, audit or access control of its own |
| Vault adapter as a configuration source (Azure Key Vault, AWS Secrets Manager, HashiCorp Vault) | Rotation, audit trail, central access control | Cloud or service dependency; startup latency; local development needs a fallback |
| Platform-injected secrets (Kubernetes secrets, container platform bindings) | No code change; platform handles delivery | Base64 is not encryption; rotation needs restarts |
| Managed identity, no secret at all | Nothing to leak or rotate | Only for services that support it; cloud specific |

**Table 34 — Abuse control and hardening options**

| Concern | Options | Trade-off |
|---------|---------|-----------|
| Rate limiting | Edge or gateway only; built-in ASP.NET Core rate limiter; a third-party library | Edge is simplest and shared; the built-in limiter gives per-endpoint or per-user control without a new dependency; a library adds a dependency for little gain |
| CORS | Allow-list from configuration; permissive during development only | An allow-list needs configuration per environment; permissive defaults leak into production |
| Input validation | Data annotations; a validator interface with an implementation library; domain checks only | Annotations are simplest; a validation library is more expressive but is a third-party dependency |
| Audit logging | Structured log category; dedicated audit store or event stream | A log category is cheap but mixed with operations logs; a store gives retention and tamper evidence at a cost |
| Supply chain | SBOM per build; vulnerability scan in CI; dependency review bot; lock files | Each adds pipeline time; together they give traceability |

Product and library capabilities reflect general knowledge and should be re-checked before a decision.

**Verdict: Keep the configuration-first model; Consider a vault adapter as an `ExternalServices` project.** Add the built-in rate limiter and an allow-list CORS extension to a shared web setup, keep validation on annotations or an injected validator, log audit events under a dedicated category, and generate an SBOM and run a vulnerability scan in CI. Record the vault choice as an ADR when a production target is known.

---

<!-- nav -->
[↑ 05 — Industry Alternatives](./README.md) · [← Authorization Approaches](./11-authorization.md) · [Observability Approaches →](./13-observability.md)
<!-- nav -->
