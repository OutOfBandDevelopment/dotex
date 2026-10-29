# Plugin sandbox threat model

**Date:** 2026-10-29

Phase 0 of the sandboxed plugin host: [docs/design/PluginSandbox/threat-model.md](../design/PluginSandbox/threat-model.md) lists assets, actors and four trust boundaries, 17 threats with mitigation and status, a review of the proving-grounds proof of concept (escape matrix, weaknesses, untested parts) and nine gaps mapped to phases. Main findings: the Linux seccomp filter only blocks process creation, there is no Linux CPU or cgroup limit, `entry.dev` and the install-folder swap window are open, there are no per-host network grants or broker yet, and the Windows launcher does not support Java.

Also written: [protocol-and-manifest.md](../design/PluginSandbox/protocol-and-manifest.md), the phase 1 design (framing, envelope with a `v` field, violation table, plugin rules, versioning, manifest fields and example).

Request: [2026-10-28-plugin-sandbox.md](../features/request/2026-10-28-plugin-sandbox.md) (still `triaged`).

**Verified:** documentation only. `OoBDev.Docs.Tests` on `docs/design/PluginSandbox`: 4 passed. The proof-of-concept results are quoted from its own findings, not re-run here. REQ-010 sign-off by the owner is pending (Q6).
