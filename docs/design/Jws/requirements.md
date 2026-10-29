# JWS Reference Implementation — Requirements

[↑ Overview](README.md) · [Architecture →](architecture.md)

**Status:** Accepted · **Date:** 2026-10-29

## Contents

- [Goals and non-goals](#goals-and-non-goals)
- [Requirements](#requirements)

### List of Tables

1. [Table 1 — Requirements](#table-1)

## Goals and non-goals

Goals: sign and verify a compact JWS with HS256 and RS256 from the RFC text; prove it correct against RFC 7515 appendix A.1 and a known-good library; make the unsafe choices (algorithm taken from the token, `none`, short keys) impossible.

Non-goals: JWE or JSON serialization, JWK parsing, key rotation or `kid` lookup, claim validation (`exp`, `aud`, `iss`), ES256 and PS256, replacing the platform bearer handler.

## Requirements

<a id="table-1"></a>**Table 1 — Requirements**

| ID | Requirement | Priority |
|----|-------------|----------|
| REQ-001 | Encode and decode base64url without padding, with `-` and `_` | Must |
| REQ-002 | `SignHs256` reproduces the RFC 7515 appendix A.1 token byte for byte | Must |
| REQ-003 | A token signed here validates in `System.IdentityModel.Tokens.Jwt`, and the HS256 signature equals the library's | Must |
| REQ-004 | Verification fails for a changed header, payload or signature, and for a wrong key | Must |
| REQ-005 | Verification fails when `alg` differs from the pinned one, including `none`, a different case and a missing `alg` | Must |
| REQ-006 | Signatures are compared with `CryptographicOperations.FixedTimeEquals` | Must |
| REQ-007 | Verification never throws on malformed input; it returns `false` | Must |
| REQ-008 | Keys below the RFC 7518 minimum (HS256 32 bytes, RSA 2048 bits) are refused | Should |
| REQ-009 | A header with `crit` is refused, because no critical extension is understood (RFC 7515 section 4.1.11) | Should |
| REQ-010 | RS256 sign and verify with `RSA`, including refusal of an HS256 token signed with the public key bytes | Should |

[Architecture →](architecture.md)
