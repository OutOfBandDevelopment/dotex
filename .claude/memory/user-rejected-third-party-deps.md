---
name: user-rejected-third-party-deps
description: "Owner rejects third-party IoC/DI containers and third-party logging; avoids Polly; prefers platform primitives, SHA-512, OpenTelemetry"
metadata:
  type: user
---

Third-party IoC/DI containers (Autofac, Lamar) and third-party logging (Serilog) are rejected. Polly is avoided because of its license change. Hash default should be SHA-512. OpenTelemetry should be added. MSTest stays. Provider selection should become a container-registered factory that picks a keyed service from a configuration path (replacing `ISelectedService<T>`, which predates keyed services).

**Why:** stated in the owner answers to the alternatives docs. Related: [[user-http-api-authz-preferences]].

**How to apply:** never suggest swapping in a third-party container or logger as a "Keep/Consider" option except to label it rejected; when comparing options, mark licensing concerns. Decisions recorded under each verdict in `docs/patterns-discovery/05-industry-alternatives/` and in a `TODO.md` backlog.
