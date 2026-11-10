---
name: user-auth-oauth-oidc-sts
description: "Auth preference: OAuth/OIDC/JWT where possible; STS with token exchange (SSO token to app-specific token) when required"
metadata:
  node_type: memory
  type: user
  originSessionId: 9ea789d6-f84c-46cc-8a8c-3bc487e0ad56
  modified: 2026-09-30T14:14:34.177Z
---

Authentication should use OAuth 2.0 / OIDC / JWT wherever possible. When an application needs its own token, support an STS that performs token exchange to convert SSO tokens into application-specific tokens. This includes Windows: a SAML assertion or Windows (Kerberos/NTLM) token should be exchanged at the STS so everything ends up on JWT. Services should only need to know how to use JWTs; any other credential (bearer, basic, API key, HMAC, etc.) is converted to JWT by the STS.

**Why:** stated directly by the user as a standing design preference, alongside [[user-ui-mvvm-preference]]. An older wish (now in `BACKLOG.md`) asks for an example exchange handling multiple client ids in one service set.

**How to apply:** default new auth designs to standards-based flows; treat token exchange as optional and config-gated, injected by interface; don't pass SSO tokens straight through to downstream services. Recorded in `docs/patterns-discovery/03-practices-and-conventions/09-authentication-practices.md` and a `TODO.md` backlog.
