using System.Windows;
using DevTermApp;

namespace OoBDev.DevTerm.Wpf;

/// <summary>Shows a <see cref="ConfigureDeviceDialogViewModel"/> in a <see cref="ConfigureDeviceWindow"/>.</summary>
public sealed class WpfConfigureDeviceDialog : IConfigureDeviceDialog
{
    /// <inheritdoc />
    public void Show(ConfigureDeviceDialogViewModel device) =>
        new ConfigureDeviceWindow(device) { Owner = Application.Current?.MainWindow }.ShowDialog();
}
