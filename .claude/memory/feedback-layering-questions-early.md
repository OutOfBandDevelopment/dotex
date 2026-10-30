---
name: feedback-layering-questions-early
description: Ask layering/composition questions and show a migration plan (projects, deployments, namespaces) before work starts; consider spikes
metadata:
  type: feedback
---

Raise layering or composition questions (framework vs application, which layer, merge vs separate, what a project may reference) as questions up front, and show a migration plan with projects, deployment units and namespaces before moving anything. Recommend a spike when the shape is unclear.

**Why:** DevTerm was ported into Framework/Extensions as many small libraries; the owner wanted one Applications core and reusable transports in System.IO.*, which meant a restructure after much work (2026-10-29).

**How to apply:** see rule `.claude/rules/layering-and-migration-plans.md`. Telemetry is generic and belongs in a host-free Framework library, not under ASP.NET-bound libraries. [[feedback-stack-one-branch]]
