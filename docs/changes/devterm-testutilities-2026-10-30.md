# DevTerm: shared user-flow test base (2026-10-30)

New `src/Applications/OoBDev.DevTerm.TestUtilities` (namespace `OoBDev.DevTerm.TestUtilities`, MSTest only) holds `UserFlowTestsBase` and `IFrontEndDriver`; the duplicate copies in `Web.Tests` and `Tui.Tests` are gone. Plan: [test-utilities-plan.md](../design/DevTermParity/test-utilities-plan.md). Owner decision 2026-10-30 (Q2).

Verification: Tui user-flow Unit run 10 passed, 1 skipped; Web user-flow run 10 passed, 1 failed (`Panel_SettingTheControlsAndPressingApply_SendsTheCommandToTheDevice`, Playwright locator count 0). Not compared with the state before the move.
