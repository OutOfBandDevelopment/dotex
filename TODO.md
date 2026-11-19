# TODO - OoBDev (dotex) Framework

> **Active work only.** The full work list is [BACKLOG.md](BACKLOG.md); finished work goes to [docs/changes/](docs/changes/README.md). Owner questions: [OPEN-QUESTIONS.md](OPEN-QUESTIONS.md).

**Last Updated:** 2026-11-19

Keep this file short (about 15 one-line items). When an item starts, move it here from the backlog; when it finishes, remove it here and from the backlog, record it in `docs/changes/`, and update the feature docs it touched.

## In progress

- [x] Owner request 2026-11-19, "do everything you can do without question": work every item that needs no answer, recording an assumption on its question when a default exists; do not stop early. Left, in order: (1) fix the stale `DatabaseTemplateSource` rows in [TextTemplating](docs/design/TextTemplating/README.md); (2) SharedFramework design documents on the defaults of Q18 to Q20 (complex events, data loader, spatial, test data, reference designs); (3) claims pipeline versus `IClaimsTransformation` comparison (Q21); (4) desired-state bake-off (Q4 default); (5) tracking audit via `work-backlog`. Done: ONNX versus HTTP comparison. Parked: Q5, Q12, Q24, plugin sandbox, hardware requests (Q13 to Q17).

## Next

- [ ] Order (owner, 2026-11-09): the cleanup follow-ups first (done: bake-off test plan, example guide cleanup, Ollama tests); the DevTerm WPF slices are all done ([status](docs/design/DevTermParity/status.md)), what is left is the decided UX work (BACKLOG section 8).

- [ ] Device decoding leftovers: DE-5000 Hardware test (needs BLE) and the physical passes; phase 0 is done ([device merge list](docs/migration/README.md#devices)).
