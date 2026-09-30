# Authentication Approaches

<!-- nav -->
[↑ 05 — Industry Alternatives](./README.md) · [← UI Patterns](./08-ui.md) · [Index →](./README.md)
<!-- nav -->

## 24. Authentication and token exchange

**Today:** JWT bearer validation and Swagger OAuth in `OoBDev.AspNetCore.JwtAuthentication`, identity helpers in `OoBDev.Identity`, and a Keycloak container for integration tests. The stated preference is OAuth 2.0, OIDC and JWT wherever possible, plus an STS that exchanges SSO tokens for application-specific tokens when required ([authentication practices](../03-practices-and-conventions/09-authentication-practices.md)).

**Table 26 — Approaches to cross-application tokens**

| Approach | Pros | Cons |
|----------|------|------|
| STS with OAuth 2.0 token exchange (RFC 8693) | Standard; tokens scoped per application; SSO token never leaves the front door; clear audit point | Needs an STS (Keycloak supports token exchange in current versions; some providers need a custom service); extra hop |
| STS that accepts Windows (Kerberos) or SAML input and issues JWT (for example AD FS or Entra ID federation, Keycloak identity brokering, or the SAML bearer assertion grant, RFC 7522) | Windows and SAML users land on JWT like everyone else; downstream services need no Windows or SAML code | The STS carries the Windows/SAML complexity (SPNs, delegation, assertion validation); on-premises setups need domain integration |
| STS or edge adapter that converts any credential (API key, HMAC signature, basic auth, legacy bearer) to JWT | Services validate JWT only; new credential types are added in one place; easy to test and audit | The STS/adapter becomes security-critical and an availability dependency; HMAC verification needs shared-secret storage and replay protection |
| Pass the SSO token through to every service | Simplest | Broad audience and scope; a leaked token opens every service; hard to revoke per application |
| On-behalf-of flow (Microsoft identity platform) | Built into Azure AD; keeps user identity | Provider-specific; not portable |
| API gateway or BFF that swaps tokens | Central control; hides tokens from browsers | Gateway becomes a bottleneck and single point of failure; logic outside the applications |
| Custom session cookies and opaque tokens | Easy revocation | Not a standard; each service needs the session store; breaks the OIDC and JWT preference |
| Mutual TLS or workload identity (service to service) | No user token needed for machine calls | Does not carry the user; complements rather than replaces token exchange |

**Table 27 — Ways to obtain an STS**

| Option | Pros | Cons |
|--------|------|------|
| Keycloak token exchange | Already used in the test infrastructure; open source | Feature availability and configuration vary by version; verify before relying on it |
| Duende IdentityServer or OpenIddict | .NET native, extensible token exchange support | Licensing (Duende) or more assembly required (OpenIddict) |
| Cloud provider service (Azure AD, AWS STS) | Managed | Provider lock-in; token exchange semantics differ |
| Small in-house STS built on the framework | Full control; fits the provider and builder patterns | Security-critical code to maintain; needs review and testing |

Availability and licensing statements reflect general knowledge and must be re-checked before a decision.

**Verdict: Keep the preference.** Build on the standards, make the STS an injected interface with a config-gated registration ([pattern 8](../02-design-patterns/08-config-gated-registration.md)), and start with an example that proves exchange for multiple client ids within one service set. Record the STS choice as an ADR.

---

<!-- nav -->
[↑ 05 — Industry Alternatives](./README.md) · [← UI Patterns](./08-ui.md) · [Index →](./README.md)
<!-- nav -->
