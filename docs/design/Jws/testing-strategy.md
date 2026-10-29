# JWS Reference Implementation — Testing strategy

[← API design](api-design.md) · [↑ Overview](README.md)

**Status:** Accepted · **Date:** 2026-10-29

## Coverage

All tests are Unit (`OoBDev.Identity.Jws.Tests`): no I/O, no services. Line coverage is above 95%, over the 80% Framework target.

### List of Tables

1. [Table 3 — Requirement coverage](#table-3)

<a id="table-3"></a>**Table 3 — Requirement coverage**

| Requirement | Tests |
|-------------|-------|
| REQ-001, REQ-002 | `SignHs256_Rfc7515AppendixA1_MatchesTheSpecificationToken`, `TryVerifyHs256_Rfc7515AppendixA1_ReturnsThePayload` |
| REQ-003 | `SignHs256_ThenVerifiedByTheLibrary_IsAccepted`, `SignHs256_ComparedWithTheLibrarySignature_IsByteIdentical`, `SignRs256_ThenVerifiedByTheLibrary_IsAccepted` |
| REQ-004 | `TryVerifyHs256_PayloadChangedAfterSigning_ReturnsFalse`, `TryVerifyHs256_WrongKey_ReturnsFalse`, `TryVerifyRs256_SignedWithAnotherKey_ReturnsFalse` |
| REQ-005, REQ-009 | `TryVerifyHs256_HeaderNotPinnedToHs256_ReturnsFalseEvenWithAValidSignature` (none, RS256, lower case, missing, crit, array), `TryVerifyHs256_NoneTokenWithEmptySignature_ReturnsFalse`, `SignHs256_HeaderWithAnotherAlgorithm_Throws` |
| REQ-006 | Code review: `FixedTimeEquals` is the only comparison; a timing test would be flaky |
| REQ-007 | `TryVerifyHs256_MalformedToken_ReturnsFalseWithoutThrowing` |
| REQ-008 | `SignHs256_ShortKey_Throws`, `TryVerifyHs256_ShortKey_ReturnsFalse`, `SignRs256_SmallKey_Throws` |
| REQ-010 | `SignRs256_ThenVerified_ReturnsThePayload`, `TryVerifyRs256_Hs256TokenSignedWithThePublicKeyBytes_ReturnsFalse` |
