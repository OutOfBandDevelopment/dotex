# TODO - OoBDev (dotex) Framework

> **Active work only.** The full work list is [BACKLOG.md](BACKLOG.md); finished work goes to [docs/changes/](docs/changes/README.md). Owner questions: [OPEN-QUESTIONS.md](OPEN-QUESTIONS.md).

**Last Updated:** 2026-10-22 · **Branch:** `dev/claude-config`

Keep this file short (a handful of items). When an item starts, move it here from the backlog; when it finishes, remove it here and from the backlog, record it in `docs/changes/`, and update the feature docs it touched.

## In progress

- [ ] OData and GraphQL surfaces: waiting on Q2 (library choice); take the next item meanwhile

## Next

- [ ] Compare designs to code with the `reconcile-designs-and-code` skill (`CgiMiddleware` done 2026-10-22; AllMiniLmL6V2, AsyncApi, HttpQueryVerb, ImageEmbeddings done 2026-10-22; remaining: ApplicationRights, LlmCodeGen, OpenApiScalar, example-ui)
- [ ] **Incoming code recommendation plan**: written ([docs/migration/incoming-recommendation-plan.md](docs/migration/incoming-recommendation-plan.md) and [cross-check](docs/migration/incoming-harvest-crosscheck.md)); waiting for the owner to answer D1 to D9 (Q1 in OPEN-QUESTIONS.md). No migration until then. The sections marked verify need a second look before work starts.

- [ ] `OoBDev.DacFx.Tests` SQLCLR test project
- [ ] Promote the clean analyzer `suggestion` rules to `warning` and fix or remove the `#warning` markers (decided)
