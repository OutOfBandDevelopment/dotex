# DevTerm WPF theme (2026-10-30)

The WPF shell follows the DevTerm theme. `WpfTheme` (ported from dev-term) maps every `ThemeRole` to a frozen brush resource (`DevTerm.<Role>`), overrides the `SystemColors` brushes for non-Light themes and merges `DarkControls` (button, combo box and menu templates) for dark themes. `MainWindow` attaches itself, `App` attaches the application resources, and `ActiveTheme.Changed` re-applies the theme to every attached window. `ThemeViewModel` and a **Theme** combo box in the shell choose a theme.

- Tests (`ThemeTests`): selecting dark changes `ActiveTheme.Current`; `Apply` adds the role brushes and the dark templates; a live switch reaches an attached window. The WPF test project has 45 Unit tests passing.
- Not done: `ThemeBuilderWindow`, `ThemeSeedPickerWindow`, the converter tools and `SvgPreview`. No window was viewed running with a dark theme.
