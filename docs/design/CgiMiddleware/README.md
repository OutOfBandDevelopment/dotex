# CGI Middleware — Overview

**Status:** Draft · **Date:** 2026-10-22 · **Related:** [docs/todo.md](../../todo.md), [design standard](../../patterns-discovery/06-design-document-standard.md)

An opt-in ASP.NET Core middleware that runs CGI/1.1 ([RFC 3875](https://www.rfc-editor.org/rfc/rfc3875)) programs as child processes, so a Kestrel application can serve legacy scripts and small native tools without changing Kestrel. The new project is `OoBDev.AspNetCore.Cgi` (name decided 2026-10-22; not created yet).

## Contents

1. [Requirements](requirements.md) — goals, non-goals, numbered requirements, open questions
2. [Architecture](architecture.md) — layer, components, request flow, configuration, alternatives
3. [API design](api-design.md) — `CgiOptions`, `UseCgi`, `TryAddCgiServices`, error model
4. [Testing strategy](testing-strategy.md) — requirement to test map, fixture scripts, benchmark

### List of Figures

1. [Figure 1 — Containers](architecture.md#figure-1)
2. [Figure 2 — Request sequence](architecture.md#figure-2)
3. [Figure 3 — Types](api-design.md#figure-3)

### List of Tables

1. [Table 1 — Requirements](requirements.md#table-1)
2. [Table 2 — Meta-variables](architecture.md#table-2)
3. [Table 3 — Alternatives](architecture.md#table-3)
4. [Table 4 — Requirement coverage](testing-strategy.md#table-4)

## Summary

One middleware (`CgiMiddleware`), one options class (`CgiOptions`), and registration through `ServiceCollectionExtensions` plus `UseCgi`. A URL prefix (default `/cgi-bin`) maps to a single script root. Safety is the default: an extension allowlist, no shell, request data only in the environment and stdin, and bounded time, concurrency, body and output.

[Requirements →](requirements.md)
