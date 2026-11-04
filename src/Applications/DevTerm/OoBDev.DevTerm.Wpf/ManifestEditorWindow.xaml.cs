using System;
using System.Windows;
using OoBDev.DevTerm.Wpf;

namespace DevTermApp;

/// <summary>Hosts a <see cref="ManifestEditorDialogViewModel"/>.</summary>
public partial class ManifestEditorWindow : Window
{
    /// <summary>Creates the window over <paramref name="editor"/>.</summary>
    /// <param name="editor">The editor view model.</param>
    public ManifestEditorWindow(ManifestEditorDialogViewModel editor)
    {
        ArgumentNullException.ThrowIfNull(editor);
        InitializeComponent();
        WpfTheme.Attach(this);
        DataContext = editor;
        Closed += (_, _) => editor.Dispose();
    }
}
