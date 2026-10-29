# JWS Reference Implementation — API design

[← Architecture](architecture.md) · [↑ Overview](README.md) · [Testing strategy →](testing-strategy.md)

**Status:** Accepted · **Date:** 2026-10-29

## Public type

`OoBDev.Identity.Jws.JsonWebSignature` (static):

```csharp
string SignHs256(string headerJson, string payloadJson, ReadOnlySpan<byte> key);
bool   TryVerifyHs256(string token, ReadOnlySpan<byte> key, out string payloadJson);
string SignRs256(string headerJson, string payloadJson, RSA privateKey);
bool   TryVerifyRs256(string token, RSA publicKey, out string payloadJson);
```

Constants: `Hs256`, `Rs256`, `MinimumHs256KeyBytes` (32), `MinimumRsaKeyBits` (2048).

## Contracts

- The header is signed exactly as given (its bytes are not re-serialized), so the RFC vectors reproduce.
- Sign methods throw `ArgumentException` for a header that is not a JSON object with the matching `alg` and no `crit`, and for a key below the minimum.
- Verify methods return `false` and an empty payload for any failure; they do not throw on token content.
- The payload is returned as the UTF-8 text that was signed; claims are not parsed or checked.
