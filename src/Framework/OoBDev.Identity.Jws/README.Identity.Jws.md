# OoBDev.Identity.Jws

A reference implementation of compact JSON Web Signatures (RFC 7515), written from the specification so the steps are visible. It is not the production authentication path: applications validate bearer tokens with the platform (`OoBDev.AspNetCore.JwtAuthentication`).

## Description

`JsonWebSignature` is a static class with no dependencies beyond the base class library.

```csharp
var token = JsonWebSignature.SignHs256("{\"alg\":\"HS256\",\"typ\":\"JWT\"}", "{\"sub\":\"1\"}", key);   // header.payload.signature
bool ok = JsonWebSignature.TryVerifyHs256(token, key, out var payload);
```

- `SignHs256` / `TryVerifyHs256` use a shared secret of at least 32 bytes; `SignRs256` / `TryVerifyRs256` use an `RSA` key of at least 2048 bits.
- Each verify method pins its algorithm. A token whose `alg` differs (including `none`, another case or a missing value), whose header has `crit`, or whose signature does not match returns `false` with an empty payload; malformed input never throws.
- Signatures are compared with `CryptographicOperations.FixedTimeEquals`.
- The header is signed exactly as given, so the RFC 7515 appendix A.1 vector reproduces byte for byte.

Not included: JWE, JSON serialization, JWK parsing, `kid` lookup, claim checks (`exp`, `aud`, `iss`), ES256 and PS256.

Design: [docs/design/Jws](../../../docs/design/Jws/README.md).
