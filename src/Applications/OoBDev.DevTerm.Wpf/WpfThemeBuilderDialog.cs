using System.Windows;
using DevTermApp;

namespace OoBDev.DevTerm.Wpf;

/// <summary>Shows the theme view models in a <see cref="ThemeSeedPickerWindow"/> and a <see cref="ThemeBuilderWindow"/>.</summary>
public sealed class WpfThemeBuilderDialog : IThemeBuilderDialog
{
    /// <inheritdoc />
    public bool PickSeed(ThemeSeedPickerViewModel picker) =>
        new ThemeSeedPickerWindow(picker) { Owner = Application.Current?.MainWindow }.ShowDialog() == true;

    /// <inheritdoc />
    public bool Edit(ThemeBuilderDialogViewModel builder) =>
        new ThemeBuilderWindow(builder) { Owner = Application.Current?.MainWindow }.ShowDialog() == true;
}
