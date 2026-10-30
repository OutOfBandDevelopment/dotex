using System.Windows;
using DevTermApp;
using OoBDev.DevTerm.Configuration;
using UiDefinitionsWpf;

namespace OoBDev.DevTerm.Wpf;

/// <summary>Shows the connection editor in a <see cref="ConnectionEditorWindow"/>.</summary>
public sealed class WpfConnectionEditorDialog(IProfileZipDialogs? zipDialogs = null) : IConnectionEditorDialog
{
    /// <inheritdoc />
    public CliOptions? Show(ConnectionEditorViewModel editor)
    {
        var window = new ConnectionEditorWindow(editor, zipDialogs is null ? null : new ProfileZipBrowser(editor, zipDialogs)) { Owner = Application.Current?.MainWindow };
        ConnectionEditorPrompts.Wire(editor, new WpfConfirmPrompt(window));
        editor.ResolveZipImportConflict = name => MessageBox.Show(
            window,
            $"A profile named '{name}' already exists.\n\nYes replaces it, No imports it under a new name, Cancel skips it.",
            "Import profiles",
            MessageBoxButton.YesNoCancel,
            MessageBoxImage.Question) switch
        {
            MessageBoxResult.Yes => ZipImportConflictResolution.Replace,
            MessageBoxResult.No => ZipImportConflictResolution.Rename,
            _ => ZipImportConflictResolution.Skip,
        };
        return window.ShowDialog() == true ? editor.Result : null;
    }
}
