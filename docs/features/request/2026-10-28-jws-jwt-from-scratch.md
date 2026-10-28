# Feature request: JWS/JWT signing and verification from scratch

- **Status:** new
- **Requested by:** owner (filed from the notes repository, `projects/dotex-consolidation/jws-jwt-from-scratch.md`)
- **Date:** 2026-10-28
- **Area:** `OoBDev.Identity` (or a new `OoBDev.Identity.Jwt`); the notes repository only suggests the name, dotex decides

## Need

The owner wants a small JSON Web Signature (RFC 7515) implementation written from the spec, so the signing and verification steps are understood and testable instead of hidden
behind a library. Today dotex only configures bearer authentication (`OoBDev.AspNetCore.JwtAuthentication`), which delegates the cryptography to the platform.

## Proposed behavior

Sign and verify a compact JWS with HS256, and prove it is correct by comparing with a known-good library.

```csharp
var token = Jws.SignHs256(headerJson, payloadJson, key);   // header.payload.signature, each part base64url
bool ok   = Jws.VerifyHs256(token, key, out var payload);  // false for a changed part, a wrong key or an unexpected alg
```

Names are placeholders. Optional second step: RS256.

## Acceptance criteria

- Given the base64url test vectors from RFC 7515, when encoded and decoded, then results match (no padding, `-` and `_` in place of `+` and `/`).
- Given the same header, payload and key, when signed here and with `System.IdentityModel.Tokens.Jwt`, then the two tokens are byte-for-byte identical.
- Given a token whose payload was changed after signing, when verified, then verification fails.
- Given a token whose `alg` header is not the one the caller pinned (including `none`), when verified, then verification fails.
- Given two signatures that differ, when compared, then the comparison is constant time (`CryptographicOperations.FixedTimeEquals`).

## Existing code checked

- `OoBDev.AspNetCore.JwtAuthentication` configures bearer authentication and OAuth2 for the OpenAPI and Scalar documents. From its README it does not sign tokens itself.
- `OoBDev.Identity` has a user management provider; I did not find any JWS code there. The source of these two was not read.
- Not checked: outside libraries beyond `System.IdentityModel.Tokens.Jwt` as the reference.

## Security and operations

Cryptography code, so it needs review. It touches no stored data and costs nothing to run. It must not become the production authentication path unless the owner decides
that; the notes treat it as a learning and reference implementation. Pin the algorithm on the verifying side to avoid algorithm-confusion attacks.

## Priority

- **Impact:** nothing is blocked.
- **Needed by:** no date.
- **Workaround today:** use `System.IdentityModel.Tokens.Jwt`.

## Open questions

- Stop at HS256, or add RS256?
- Home: inside `OoBDev.Identity`, or a new `OoBDev.Identity.Jwt` package?
- Does it ship as a package, or live as tests and a how-to only?

## Triage notes

Maintainers fill this in.
