# DevTerm WPF: menu bar and Theme menu (2026-10-30)

`MainWindow.xaml` got a File, Device and View menu bar bound to the existing shell and session commands. View has a checkable Theme submenu: `ThemeViewModel.Choices` holds one `ThemeChoice` per theme (name, `IsSelected`, a `SelectThemeCommand`), and the check mark follows the selection. Not ported: the Software flow control toggle (no session option exists for it yet).

Verification: WPF Unit suite 117 passed (2 new in `ThemeTests`). The menu bar was not viewed running.
