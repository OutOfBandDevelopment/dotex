# Migration plan: OoBDev.DevTerm.TestUtilities

Owner decision 2026-10-30 (Q2, option A).

**Table 1 — Projects**

| Item | Current | Target | Namespace | Unit | References |
|------|---------|--------|-----------|------|------------|
| `UserFlowTestsBase` | copies in `Tui.Tests` and `Web.Tests` | `src/Applications/OoBDev.DevTerm.TestUtilities` | `OoBDev.DevTerm.TestUtilities` | test library, not packed | MSTest.TestFramework only |
| `IFrontEndDriver` | copies in `Tui.Tests` and `Web.Tests` | same project | same | same | none |
| `Tui.Tests`, `Web.Tests` | own copies | delete copies | unchanged | unchanged | add `ProjectReference` to the new project; `using OoBDev.DevTerm.TestUtilities;` |

Later candidates: `FakeDeviceConfigEditor`, `FakeTransport`, `ManualTimeProvider` (they need Core references, so each is its own step).
