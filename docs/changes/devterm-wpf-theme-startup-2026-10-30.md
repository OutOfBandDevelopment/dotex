# WPF theme start-up (2026-10-30)

## What changed

- `ThemeStartup.Initialize` (WPF project) layers the DevTerm configuration, selects `--theme` / `DEVTERM_THEME`, else the saved preference, else the system theme, and makes later View > Theme choices persist. `App.OnStartup` calls it before `WpfTheme.AttachApplication`.
- `ThemeStartup.FollowSystemChanges` re-resolves the "system" theme when Windows changes its colors.
- Tests: `ThemeStartupTests` (2 Unit; command-line choice is not saved, a menu choice survives a restart) and the existing `ThemeTests` pass (7 in all).
- The Ebyte `IDeviceConfigEditor` moved to the future ideas: dev-term has only the interface and a fake, so it is not part of parity.

## Not done

- The theme was not viewed in a running WPF window.
