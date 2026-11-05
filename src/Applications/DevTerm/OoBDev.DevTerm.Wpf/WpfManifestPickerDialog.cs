using System.Windows;
using DevTermApp;

namespace OoBDev.DevTerm.Wpf;

/// <summary>Shows the manifest picker view model in a <see cref="ManifestPickerWindow"/>.</summary>
public sealed class WpfManifestPickerDialog : IManifestPickerDialog
{
    /// <inheritdoc />
    public bool Pick(ManifestPickerDialogViewModel picker) =>
        new ManifestPickerWindow(picker) { Owner = Application.Current?.MainWindow }.ShowDialog() == true;
}
