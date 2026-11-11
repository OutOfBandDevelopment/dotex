# Feature request: authentication translation gateway

- **Status:** triaged
- **Requested by:** owner (notes repository project mapping, 2026-11-10)
- **Date:** 2026-11-10
- **Area:** `OoBDev.AspNetCore.JwtAuthentication`, `OoBDev.Identity*`, a new gateway project

## Need

Services in one system accept different credentials: JWT bearer tokens, OAuth2 access tokens, API keys, plain header keys and client certificates. A
caller that has one kind often needs to call a service that wants another. The notes backlog describes an ASP.NET Core proxy that converts the inbound
credential into the one the target service expects, using a table of known service contracts. dotex validates JWT bearer tokens but has no conversion.

## Proposed behavior

A reverse-proxy middleware or small host that authenticates the inbound request with one scheme, then calls the target with another. A configuration
entry per target names the outbound scheme and where its secret comes from. Example: inbound API key `X-Api-Key` becomes outbound `Authorization: Bearer <token>`
obtained from the configured identity provider.

## Acceptance criteria

- Given a valid inbound API key and a target configured for bearer tokens, when proxied, then the target receives a valid bearer token and the response is returned.
- Given an invalid or missing inbound credential, when proxied, then the request is rejected before any outbound call.
- Given a token that has expired, when proxied, then a new one is requested once and cached until near expiry.
- Given a target with no configured contract, when proxied, then the request is refused.
- Given any outcome, then secrets and tokens are not written to logs.

## Existing code checked

- `OoBDev.AspNetCore.JwtAuthentication`: JWT bearer validation and OAuth2 sign-in for OpenAPI and Scalar. No other inbound schemes and no outbound conversion.
- `OoBDev.Identity`, `OoBDev.Identity.Jws`: tokens and signing; the JWS request (`2026-10-28-jws-jwt-from-scratch.md`) covers building and checking tokens, not proxying.
- `2026-01-20-lightweight-auth-provider.md` and `2026-11-10-claims-enhancement-and-user-session.md`: adjacent, neither converts credentials.
- The Keycloak external service and the Example application: a token source to convert to.
- Not checked: existing proxies (YARP and similar) that could do the forwarding with only the authentication step written here.

## Security and operations

High sensitivity: the gateway holds secrets and mints credentials on behalf of callers. Needs an explicit allow-list of targets (no open proxy), secret
storage outside the repository, short token cache lifetimes, an audit entry per conversion, and TLS to both sides. Costs one extra hop per call.

## Priority

- **Impact:** none inside dotex today; enables the notes backlog project.
- **Needed by:** no date
- **Workaround today:** per-service client code.

## Open questions

- Build on YARP (forwarding done) or an own `HttpClient` gateway as the notes backlog sketches?
- Which inbound schemes first: API key, header key, client certificate, token exchange (RFC 8693)?
- Does it belong in dotex at all, or in a separate application that references it?

## Triage notes

Origin: notes repository backlog (`projects/project-ideas.md`, Software Architecture & APIs, SSO/Authentication Translation Proxy). Maintainers: accept, schedule or decline.

2026-11-11: triaged. Accepted as a backlog idea: design first (`docs/patterns-discovery/06-design-document-standard.md`), no code until a consumer appears. Backlog: BACKLOG.md, Consolidation requests.
