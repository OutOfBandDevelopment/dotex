# DevTerm docs: reusable designs folded (2026-11-04)

Second slice of "DevTerm docs into living documents": `ui-definitions`, `device-manifests`, `device-control-modules` (to `docs/design/UiDefinitions/`), `presenters` (`Presenters/`), `session-logging` (`SessionLogging/`) and `theming` (`Theming/`) moved out of `docs/applications/devterm/design/`, each folder with a README index. A script rewrote every relative link in the moved files and in the files linking to them (imported history excluded). Table 2 of `docs/migration/devterm-docs-map.md` tracks it.

Verified: the docs Unit tests show no broken link or PlantUML problem outside the imported history. Remaining in the app design folder: architecture, connection profiles, frontends, multi-session, observability, plugin model, platform and testing.
