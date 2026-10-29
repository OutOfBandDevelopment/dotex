# JWS reference implementation

**Date:** 2026-10-29

New Framework project `OoBDev.Identity.Jws` (with `OoBDev.Identity.Jws.Tests`): the static `JsonWebSignature` class signs and verifies compact JWS tokens with HS256 and RS256, written from RFC 7515 and 7518. Verify methods pin the algorithm (a token cannot pick `none` or another one), refuse a header with `crit`, refuse keys under 32 bytes (HS256) or 2048 bits (RSA), compare with `FixedTimeEquals` and return `false` instead of throwing. It is a learning and reference implementation, not a replacement for the bearer handler in `OoBDev.AspNetCore.JwtAuthentication`.

Request: [docs/features/request/2026-10-28-jws-jwt-from-scratch.md](../features/request/2026-10-28-jws-jwt-from-scratch.md), now `done`. Design: [docs/design/Jws](../design/Jws/README.md). Readme: `README.Identity.Jws.md`.

**Verified:** `OoBDev.Identity.Jws.Tests` Unit, 28 passed, line coverage 98%. The RFC 7515 appendix A.1 token reproduces byte for byte; the HS256 signature equals `Microsoft.IdentityModel.Tokens`'s; tokens from both algorithms validate in `JwtSecurityTokenHandler`; changed payload, wrong key, `none`, wrong-case `alg`, `crit`, malformed input and the RS256/HS256 public-key confusion all fail. The constant-time comparison is by code review, not a timing test.
