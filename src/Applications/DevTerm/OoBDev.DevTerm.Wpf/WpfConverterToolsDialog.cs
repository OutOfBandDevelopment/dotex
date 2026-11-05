using System.Windows;
using DevTermApp;

namespace OoBDev.DevTerm.Wpf;

/// <summary>Shows the converter tools view model in a <see cref="ConverterToolsWindow"/>.</summary>
public sealed class WpfConverterToolsDialog : IConverterToolsDialog
{
    /// <inheritdoc />
    public bool Edit(ConverterToolsDialogViewModel tools) =>
        new ConverterToolsWindow(tools) { Owner = Application.Current?.MainWindow }.ShowDialog() == true;
}
