# DevTerm docs: transports folded into a reusable design (2026-11-04)

First slice of "DevTerm docs into living documents": `transports.md`, `rfc2217.md`, `vxi11-transport.md` and `usbtmc-transport.md` moved from `docs/applications/devterm/design/` to `docs/design/Transports/` (with a README index); their links and every inbound link outside the imported history (`source/`, `changes/`, `bugs/`) were rewritten. Progress is tracked in Table 2 of `docs/migration/devterm-docs-map.md`.

Verified: the docs Unit tests report no broken link outside the imported history. The same tests still fail on that history, which the planned cleanup after all migrations handles. Also fixed the broken `../../BACKLOG.md` link in `user-guide/managing-profiles.md`. Remaining folders to fold: presenters, UI definitions, device manifests, session logging, theming, observability, plugin model, connection profiles, frontends, multi-session, device control modules, platform, testing and architecture.
