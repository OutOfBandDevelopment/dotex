# JWS Reference Implementation — Overview

**Status:** Accepted · **Date:** 2026-10-29 · **Request:** [JWS/JWT from scratch](../../features/request/2026-10-28-jws-jwt-from-scratch.md) · **Related:** [authentication practices](../../patterns-discovery/03-practices-and-conventions/09-authentication-practices.md), [design standard](../../patterns-discovery/06-design-document-standard.md)

A small JSON Web Signature (RFC 7515) signer and verifier written from the specification, so the signing and verification steps are understood and testable instead of hidden behind a library. It is a learning and reference implementation, not the production authentication path: applications keep validating bearer tokens with the platform (`OoBDev.AspNetCore.JwtAuthentication`).

## Contents

1. [Requirements](requirements.md) — goals, non-goals, numbered requirements
2. [Architecture](architecture.md) — placement, verification flow, alternatives
3. [API design](api-design.md) — the public type and its contracts
4. [Testing strategy](testing-strategy.md) — requirement to test map

### List of Figures

1. [Figure 1 — Verification flow](architecture.md#figure-1)

### List of Tables

1. [Table 1 — Requirements](requirements.md#table-1)
2. [Table 2 — Alternatives](architecture.md#table-2)
3. [Table 3 — Requirement coverage](testing-strategy.md#table-3)

## Summary

`OoBDev.Identity.Jws` (Framework, no dependencies beyond the base class library) holds one static class, `JsonWebSignature`, with `SignHs256`, `TryVerifyHs256`, `SignRs256` and `TryVerifyRs256`. Every verify method pins the algorithm the caller expects, so a token cannot pick its own, and a token with `alg` of `none` never verifies.

[Requirements →](requirements.md)
