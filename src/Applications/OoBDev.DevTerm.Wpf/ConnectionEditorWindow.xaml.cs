using System;
using System.Windows;
using OoBDev.DevTerm.Configuration;

namespace DevTermApp;

/// <summary>Hosts a <see cref="ConnectionEditorViewModel"/>; closes when the editor reports a valid connection.</summary>
public partial class ConnectionEditorWindow : Window
{
    /// <summary>Creates the window over <paramref name="editor"/>.</summary>
    /// <param name="editor">The editor view model.</param>
    public ConnectionEditorWindow(ConnectionEditorViewModel editor)
    {
        ArgumentNullException.ThrowIfNull(editor);
        InitializeComponent();
        DataContext = editor;
        editor.CloseRequested += (_, _) => DialogResult = true;
    }
}
