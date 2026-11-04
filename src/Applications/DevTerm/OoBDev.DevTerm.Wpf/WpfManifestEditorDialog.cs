using System.Windows;
using DevTermApp;

namespace OoBDev.DevTerm.Wpf;

/// <summary>Shows a <see cref="ManifestEditorDialogViewModel"/> in a <see cref="ManifestEditorWindow"/>.</summary>
public sealed class WpfManifestEditorDialog : IManifestEditorDialog
{
    /// <inheritdoc />
    public void Show(ManifestEditorDialogViewModel editor) =>
        new ManifestEditorWindow(editor) { Owner = Application.Current?.MainWindow }.ShowDialog();
}
