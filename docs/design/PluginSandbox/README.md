# Sandboxed Plugin Host — Overview

**Status:** Draft · **Date:** 2026-10-29 · **Request:** [Sandboxed plugin host](../../features/request/2026-10-28-plugin-sandbox.md) · **Related:** `OoBDev.AspNetCore.Cgi` (process limits, no sandbox), [design standard](../../patterns-discovery/06-design-document-standard.md)

A .NET 10 host that runs plugins written in any language as isolated processes, with approved permissions, brokered access to external resources, network isolation and resource limits.

## Contents

1. [Requirements](requirements.md)
2. [Architecture](architecture.md)
3. [API design](api-design.md)
4. [Testing strategy](testing-strategy.md)

### List of Tables

1. [Table 1 — Requirements](requirements.md#table-1)
2. [Table 2 — Phases](architecture.md#table-2)
3. [Table 3 — Requirement coverage](testing-strategy.md#table-3)

### List of Figures

1. [Figure 1 — Host, broker and plugin](architecture.md#figure-1)

## Summary

The whole feature is a security boundary, so the first deliverable is a threat model and a review of the proving-grounds proof of concept (`examples/plugin-sandbox-poc`), not code. The proof of concept is read-only reference material. Nothing in the notes' reference code was ever compiled. Whether this lives in dotex or stays a separate project is owner question Q6 in `OPEN-QUESTIONS.md`; until it is answered only the threat model and the protocol design proceed.
