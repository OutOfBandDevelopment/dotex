# Sandboxed Plugin Host — Overview

**Status:** Draft (phase 0 threat model written, awaiting owner sign-off) · **Date:** 2026-10-29 · **Request:** [Sandboxed plugin host](../../features/request/2026-10-28-plugin-sandbox.md) · **Related:** `OoBDev.AspNetCore.Cgi` (process limits, no sandbox), [design standard](../../patterns-discovery/06-design-document-standard.md)

A .NET 10 host that runs plugins written in any language as isolated processes, with approved permissions, brokered access to external resources, network isolation and resource limits.

## Contents

1. [Requirements](requirements.md)
2. [Architecture](architecture.md)
3. [API design](api-design.md)
4. [Testing strategy](testing-strategy.md)
5. [Threat model and proof-of-concept review](threat-model.md)

### List of Tables

1. [Table 1 — Requirements](requirements.md#table-1)
2. [Table 2 — Phases](architecture.md#table-2)
3. [Table 3 — Requirement coverage](testing-strategy.md#table-3)
4. [Table 4 — Threats and status](threat-model.md#table-4)
5. [Table 5 — Escape matrix of the proof of concept](threat-model.md#table-5)
6. [Table 6 — Gaps](threat-model.md#table-6)

### List of Figures

1. [Figure 1 — Host, broker and plugin](architecture.md#figure-1)
2. [Figure 2 — Trust boundaries](threat-model.md#figure-2)

## Summary

The whole feature is a security boundary, so the first deliverable is a threat model and a review of the proving-grounds proof of concept (`examples/plugin-sandbox-poc`), not code. The proof of concept is read-only reference material. Nothing in the notes' reference code was ever compiled. Whether this lives in dotex or stays a separate project is owner question Q6 in `OPEN-QUESTIONS.md`; until it is answered only the threat model and the protocol design proceed.
