using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using OoBDev.DevTerm.Configuration;

namespace DevTermApp;

/// <summary>Hosts a <see cref="ConnectionEditorViewModel"/>; closes when the editor reports a valid connection.</summary>
public partial class ConnectionEditorWindow : Window
{
    private readonly ConnectionEditorViewModel _editor;

    /// <summary>Creates the window over <paramref name="editor"/>.</summary>
    /// <param name="editor">The editor view model.</param>
    public ConnectionEditorWindow(ConnectionEditorViewModel editor)
    {
        ArgumentNullException.ThrowIfNull(editor);
        _editor = editor;
        InitializeComponent();
        DataContext = editor;
        editor.CloseRequested += (_, _) => DialogResult = true;
        editor.ProfilesChangedExternally += (_, _) => Dispatcher.BeginInvoke(() => editor.RefreshCommand.Execute(null));
    }

    private void ProfilesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _editor.SelectedProfileNames.Clear();
        foreach (var name in ((ListBox)sender).SelectedItems.OfType<string>())
        {
            _editor.SelectedProfileNames.Add(name);
        }
    }
}
