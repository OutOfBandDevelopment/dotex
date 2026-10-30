# OoBDev.DevTerm.TestUtilities

Shared test base for the DevTerm front ends. It references no DevTerm, device, transport or web project.

- `IFrontEndDriver`: one front end (CLI, TUI, WPF or web) driven the way a user drives it.
- `UserFlowTestsBase`: the user flows from `docs/user-guide`, written once. Each front end's test project derives a `[TestClass]` and supplies `RunAsync`.

Used by `OoBDev.DevTerm.Tui.Tests` and `OoBDev.DevTerm.Web.Tests`.
