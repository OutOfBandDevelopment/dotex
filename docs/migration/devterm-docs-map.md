# DevTerm documentation map

Transient. Maps every imported dev-term documentation folder to its living home in this repository; delete this file when the last row is done. Source: `C:\repo\oobdev\dev-term\docs` at `8c66e74`. Decisions and the other surroundings: [surroundings review](devterm-surroundings-review-2026-10-30.md).

**Table 1 — Where each folder goes**

| dev-term path | Now at | Living home (target) | State |
|---------------|--------|----------------------|-------|
| `docs/design/*.md` (architecture, transports, presenters, plugin model, frontends, theming, session logging, UI definitions, manifests, observability) | `docs/applications/devterm/design/` | Fold into `architecture.md` and `api-design.md` of the app; reusable parts (transports, presenters, UI definitions, manifests, session logging) become `docs/design/<Feature>/` documents | Folded 2026-11-04 (capability designs in docs/design, five app docs stay) |
| `docs/design/features`, `proposals`, `rejected` | `docs/applications/devterm/design/` | Built features: into the feature documents; proposals: `BACKLOG.md` items; rejected: one line in the feature document | Folded 2026-11-04 |
| `docs/design/features/*-protocol.md`, `usbtmc-protocol-conformance.md` | `docs/protocols/` | Done (protocol references live there) | Moved |
| `docs/protocols/usbtmc` | `docs/protocols/usbtmc` | Done | Moved |
| `docs/specs` | `docs/applications/devterm/specs/` | Stays as the app's screen specs | Kept 2026-11-04 (final location) |
| `docs/user-guide` | `docs/applications/devterm/user-guide/` | Stays; generated guide and screenshots from the app tests go here (rule `work-tracking`) | Kept 2026-11-04 (final location) |
| `docs/devices` | `docs/applications/devterm/devices/` and `src/Extensions/OoBDev.Scpi/docs` | Device notes live with the device libraries; the copy is deleted when each note is confirmed there | Done 2026-11-04: copies deleted, library copies are the source, index in docs/how-tos/test-hardware.md |
| `docs/test` | `docs/applications/devterm/test/` | Bench reports stay as evidence; new ones from the `hardware-test` skill | Kept 2026-11-04 (final location) |
| `docs/bugs`, `docs/changes` | `docs/applications/devterm/bugs`, `changes` | History only; delete after the owner agrees | Imported |
| `docs/coding-standards.md`, `CLAUDE.md`, `TODO.md`, `BACKLOG.md`, `OPEN_QUESTIONS.md` | `coding-standards.md`, `source/` | Rule `coding-standards`; open work is in `BACKLOG.md`; snapshots deleted when folded | Imported |
| `docs/design` requirements, API, testing (written here) | `docs/applications/devterm/` | Living documents | Done |

Links inside `source/` and `changes/` still use dev-term paths; they are history and are not repaired.

**Table 2 — Folded so far**

| Document | Now at | Date |
|----------|--------|------|
| transports, rfc2217, vxi11-transport, usbtmc-transport | `docs/design/Transports/` | 2026-11-04 |
| ui-definitions, device-manifests, device-control-modules | `docs/design/UiDefinitions/` | 2026-11-04 |
| presenters | `docs/design/Presenters/` | 2026-11-04 |
| session-logging | `docs/design/SessionLogging/` | 2026-11-04 |
| theming | `docs/design/Theming/` | 2026-11-04 |
| plugin-model | `docs/design/Plugins/` | 2026-11-04 |
| connection-profiles | `docs/design/ConnectionProfiles/` | 2026-11-04 |
| multi-session-ui | `docs/design/MultiSessionUi/` | 2026-11-04 |
| out-of-process-plugins, plugin-contributed-panels | `docs/design/Plugins/` | 2026-11-04 |
| message-broker-protocols | `docs/design/Transports/` | 2026-11-04 |
| cross-process-control-channel | `docs/design/SessionChannel/` | 2026-11-04 |
| network-device-discovery, network-device-config-editors | `docs/design/NetworkDevices/` | 2026-11-04 |
| project-state | `docs/design/ProjectState/` | 2026-11-04 |
| web-tunnel-blazor-frontend | `docs/design/WebFrontend/` | 2026-11-04 |
| z-wave-support | `docs/design/ZWave/` | 2026-11-04 |
| features: lxi-support, usbtmc-bulk-in-reassembly-fix, loopback-sample-rate | `docs/design/Transports/` | 2026-11-04 |
| features: theme-builder | `docs/design/Theming/` | 2026-11-04 |
| features: expression-picker-paths-and-cel, manifest-editor-expression-builder, ksy-importer, format-schema-files | `docs/design/UiDefinitions/` | 2026-11-04 |
| features: stream-content-detection, stream-converter-tools | `docs/design/StreamTools/` | 2026-11-04 |
| features: scpi-instrument-control | `docs/design/ScpiInstruments/` | 2026-11-04 |
| rejected: favero and saint-george fencing protocols | `docs/protocols/rejected/` | 2026-11-04 |
| de5000-lcr-meter-protocol, ebyte-e810-dtu-config-protocol, bytecc-bt-up01-usb-network-bridge | `docs/protocols/` | 2026-11-04 |
