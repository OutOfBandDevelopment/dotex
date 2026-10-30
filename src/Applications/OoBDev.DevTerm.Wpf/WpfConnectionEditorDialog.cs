using System.Windows;
using DevTermApp;
using OoBDev.DevTerm.Configuration;

namespace OoBDev.DevTerm.Wpf;

/// <summary>Shows the connection editor in a <see cref="ConnectionEditorWindow"/>.</summary>
public sealed class WpfConnectionEditorDialog : IConnectionEditorDialog
{
    /// <inheritdoc />
    public CliOptions? Show(ConnectionEditorViewModel editor)
    {
        var window = new ConnectionEditorWindow(editor) { Owner = Application.Current?.MainWindow };
        return window.ShowDialog() == true ? editor.Result : null;
    }
}
