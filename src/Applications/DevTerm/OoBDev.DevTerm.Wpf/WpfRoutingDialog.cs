using System.Windows;
using DevTermApp;

namespace OoBDev.DevTerm.Wpf;

/// <summary>Shows the routing view model in a <see cref="RoutingWindow"/>.</summary>
public sealed class WpfRoutingDialog : IRoutingDialog
{
    /// <inheritdoc />
    public void Show(RoutingDialogViewModel routing) =>
        new RoutingWindow(routing) { Owner = Application.Current?.MainWindow }.ShowDialog();
}
