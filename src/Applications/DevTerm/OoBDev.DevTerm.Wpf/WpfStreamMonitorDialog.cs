using System.Windows;
using DevTermApp;

namespace OoBDev.DevTerm.Wpf;

/// <summary>Shows the stream monitor view model in a <see cref="StreamMonitorWindow"/>.</summary>
public sealed class WpfStreamMonitorDialog : IStreamMonitorDialog
{
    /// <inheritdoc />
    public void Show(StreamMonitorDialogViewModel monitor) =>
        new StreamMonitorWindow(monitor) { Owner = Application.Current?.MainWindow }.ShowDialog();
}
