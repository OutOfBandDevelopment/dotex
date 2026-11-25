# DevTerm parity: testing strategy

**Status:** Draft · **Date:** 2026-10-29 · **Related:** [Requirements](requirements.md) · [API design](api-design.md) · [Architecture](architecture.md)

## Contents

- [Coverage targets](#coverage-targets)
- [Requirement traceability](#requirement-traceability)
- [Strategy](#strategy)
- [Open questions](#open-questions)

### List of Tables

- Table 1 — Requirement to test mapping

## Coverage targets

- Framework transport libraries (`OoBDev.System.IO.*`): at least 80% line coverage.
- `OoBDev.DevTerm.Core`: 85%.
- Front ends: behavior tests per screen; no percentage gate.

## Requirement traceability

**Table 1 — Requirement to test mapping**

| Requirement | Tests | Category |
|-------------|-------|----------|
| REQ-001, REQ-002 | `OoBDev.DevTerm.Core.Tests` session, router and pipeline tests over `FakeTransport` and the loopback adapter | Unit |
| REQ-003 | Text presenter tests in `Core.Tests` | Unit |
| REQ-004 | Manifest parsing, `.ksy` import and generic panel tests | Unit |
| REQ-005 | `DeviceConfigViewModelTests`, `LxiDeviceOptionTests` and editor view model tests | Unit |
| REQ-006 | Session log and playback tests | Unit |
| REQ-007 | Plugin loader and `PluginTrustProcessTests` | Unit |
| REQ-008, REQ-009 | Schema generator and `OoBDev.Telemetry.Tests` | Unit |
| REQ-010, REQ-011 | Adapter tests per `System.IO.*` library: open, fault, close, pipe reads; loopback server for RFC 2217 and TCP | Unit |
| REQ-010, REQ-011 (real device or broker) | Serial, HID, USBTMC and BLE against hardware; MQTT, AMQP and STOMP against the Docker brokers | Hardware, Integration |
| REQ-012 | Loopback adapter tests | Unit |
| REQ-013, REQ-015 | Per-device `.Panels` tests in each device test project; `OoBDev.DevTerm.Devices.Tests` for the bundle | Unit |
| REQ-020 | `OoBDev.DevTerm.Tui.Tests`: headless Terminal.Gui runs, panel layout, fit and screenshot tests | Unit |
| REQ-021 | `OoBDev.DevTerm.Web.Tests`: panel API, hub and AsyncAPI tests; Playwright flows | Unit, DevLocal |
| REQ-022 | `OoBDev.DevTerm.Wpf.Tests` | Unit |
| REQ-023 | Doc screenshot tests write to `artifacts/doc-images/tui` | Unit |
| REQ-030 to REQ-033 | Docs tests, the build (analyzers, MA0048) and layering checks | Unit |

## Strategy

- Mocks: Moq strict where a mock is needed; adapters use `FakeTransport` or a loopback server rather than mocks.
- One category per test. Hardware, broker and Playwright tests end `Assert.Inconclusive` when the device is absent and are never reported as passed.
- Test properties come from `TestContext`; new ones go in `src/.runsettings` and `TEST_VARIABLES.md`.
- Docker services: the MQTT, RabbitMQ (AMQP and STOMP) containers of `containers/testing`.
- Do not run two `dotnet test` jobs on the DevTerm projects at once (shared output files lock).

## Open questions

- Q2 in `OPEN-QUESTIONS.md`: the shared test utilities project would remove the duplicated `UserFlowTestsBase` and `IFrontEndDriver`.
