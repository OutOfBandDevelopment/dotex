# Out-of-Band Development - Example Application

## Summary

This is a collection of projects with examples on how this framework may be used to build applications.

This project may not demonstate every framework feature today but I plan on getting there sometime.  

-Thanks,

Matthew Whited

Out-of-Band Development
## Entry-point applications

| Group | Application | What it shows | Readme |
|-------|-------------|---------------|--------|
| Full framework stack | `OoBDev.Example.AppHost` | Aspire host that starts the API, the SPA and their backing services (start here) | [README.Example.AppHost.md](OoBDev.Example.AppHost/README.Example.AppHost.md) |
| Full framework stack | `OoBDev.Example.Web` | Vue 3 front end, one screen per framework feature | [README.Example.Web.md](OoBDev.Example.Web/README.Example.Web.md) |
| Full framework stack | `OoBDev.Example.WebApi` | ASP.NET Core API the SPA calls | [README.Example.WebApi.md](OoBDev.Example.WebApi/README.Example.WebApi.md) |
| Full framework stack | `OoBDev.Example.DB` | SQL Server database project with vector columns | [README.Example.DB.md](OoBDev.Example.DB/README.Example.DB.md) |
| Database | `OoBDev.Example.GraphQueries.Tests` | Graph query scripts and tests for SQL Server, PostgreSQL and SQLite | [README](OoBDev.Example.GraphQueries.Tests/README.Example.GraphQueries.Tests.md) |
| Control examples | `OoBDev.Examples.Controls.Tui` | UiDefinitions form in Terminal.Gui | [README.Controls.Tui.md](OoBDev.Examples.Controls.Tui/README.Controls.Tui.md) |
| Control examples | `OoBDev.Examples.Controls.WinForms` | Custom Windows Forms controls and a UiDefinitions form | [README.Controls.WinForms.md](OoBDev.Examples.Controls.WinForms/README.Controls.WinForms.md) |
| Control examples | `OoBDev.Examples.Controls.Wpf` | UiDefinitions form in WPF | [README.Controls.Wpf.md](OoBDev.Examples.Controls.Wpf/README.Controls.Wpf.md) |
| Control examples | `OoBDev.Examples.Controls.Blazor` | UiDefinitions form in Blazor Server | [README.Controls.Blazor.md](OoBDev.Examples.Controls.Blazor/README.Controls.Blazor.md) |
| Control examples | `OoBDev.Examples.Controls.Avalonia` | UiDefinitions form and live displays in Avalonia | [README.Controls.Avalonia.md](OoBDev.Examples.Controls.Avalonia/README.Controls.Avalonia.md) |

The full-stack group exercises the framework end to end; the control examples share one settings class (`OoBDev.Examples.Controls.Shared`) so each front end renders the same form. Test helpers: [README.Examples.TestUtilities.md](OoBDev.Examples.TestUtilities/README.Examples.TestUtilities.md).

## Control examples

`OoBDev.Examples.Controls.Wpf` renders a generated UiDefinitions form in WPF; see [README.Controls.Wpf.md](OoBDev.Examples.Controls.Wpf/README.Controls.Wpf.md). Tui and Blazor siblings are on the backlog.
