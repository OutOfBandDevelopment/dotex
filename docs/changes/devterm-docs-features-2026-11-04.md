# DevTerm docs: features and rejected folded (2026-11-04)

The imported `features/` and `rejected/` folders are empty. Feature designs moved to `docs/design/` (Transports, Theming, UiDefinitions, plus new StreamTools and ScpiInstruments), the two rejected fencing protocols to `docs/protocols/rejected/`. Stale `docs/design/...` paths in source comments, project readmes, rules and skills were repointed to the new locations (comment and text changes only; no build run). Progress is in Table 2 of [the docs map](../migration/devterm-docs-map.md).

- Verified: the docs validator reports no problem outside the imported DevTerm history.
- Not done: the architecture, frontends, platform, observability and testing designs remain in `docs/applications/devterm/design/` as the application's own documents.
