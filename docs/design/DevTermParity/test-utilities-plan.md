# Migration plan: OoBDev.DevTerm.TestUtilities

Owner decision 2026-10-30 (Q2, option A).

**Table 1 — Projects**

| Item | Current | Target | Namespace | Unit | References |
|------|---------|--------|-----------|------|------------|
| `UserFlowTestsBase` | copies in `Tui.Tests` and `Web.Tests` | `src/Applications/OoBDev.DevTerm.TestUtilities` | `OoBDev.DevTerm.TestUtilities` | test library, not packed | MSTest.TestFramework only |
| `IFrontEndDriver` | copies in `Tui.Tests` and `Web.Tests` | same project | same | same | none |
| `Tui.Tests`, `Web.Tests` | own copies | delete copies | unchanged | unchanged | add `ProjectReference` to the new project; `using OoBDev.DevTerm.TestUtilities;` |

Done 2026-10-30: `ManualTimeProvider` and `FakeDeviceConfigEditor` (three and two copies became one; the project now references Core, and `Core.Tests` and `Tui.Tests` add a global using). Also `GuideImage` (guide screenshots through `image-mask sync`).

Done 2026-11-03: `FakeTransport` (four copies in `Core.Tests` Logging and DeviceManifests, `Devices.Tests` and `Tui.Tests` became one superset: records writes and text, replies to every write, fails every or the next write or open, reopens with a fresh pipe after a close, reports line status). `Devices.Tests` adds the project reference and a global using.
