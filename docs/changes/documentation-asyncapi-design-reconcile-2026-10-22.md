# AsyncAPI design reconciled with the code

**Date:** 2026-10-22 · **Compared:** `docs/design/AsyncApi/*` against `src/Framework/OoBDev.AsyncApi`, `AsyncApiEndpointRouteBuilderExtensions`, the three adapter contributors and `AsyncApiDocumentTests`.

## Design updated to match the code

- API design: `AsyncApiComponents` and `AsyncApiReference`, the builder members, the nullable `Describe` result, the `AsyncApiContribution` record and the configuration defaults.
- Architecture: contributors are chosen by `ProviderKey`; document names come from assemblies with handlers (send-only queues appear only in `all`); providers without a contributor still get a channel.
- Testing strategy: the Simulate suite (7 tests) is named; the unit tests are marked Planned.

## Design wanted, code lacks

The isolated unit tests are in `BACKLOG.md`.

## Verification

`OoBDev.Docs.Tests` run on `docs/design/AsyncApi`.
