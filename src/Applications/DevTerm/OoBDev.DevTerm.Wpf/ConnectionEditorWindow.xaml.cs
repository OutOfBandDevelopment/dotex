using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using OoBDev.DevTerm.Configuration;
using OoBDev.DevTerm.Wpf;
using OoBDev.UiDefinitions.Forms;
using OoBDev.UiDefinitions.ViewModels;

namespace DevTermApp;

/// <summary>Hosts a <see cref="ConnectionEditorViewModel"/>; closes when the editor reports a valid connection.</summary>
public partial class ConnectionEditorWindow : Window
{
    private readonly ConnectionEditorViewModel _editor;

    /// <summary>Creates the window over <paramref name="editor"/>.</summary>
    /// <param name="editor">The editor view model.</param>
    /// <param name="zipBrowser">The zip path pickers, or null when the host has none.</param>
    public ConnectionEditorWindow(ConnectionEditorViewModel editor, ProfileZipBrowser? zipBrowser = null)
    {
        ArgumentNullException.ThrowIfNull(editor);
        _editor = editor;
        ZipBrowser = zipBrowser;
        Form = new FormViewModel(editor.FormDefinition, new FormBinding(editor));
        InitializeComponent();
        WpfTheme.Attach(this);
        DataContext = editor;
        Closed += (_, _) => Form.Dispose();
        editor.CloseRequested += (_, _) => DialogResult = true;
        editor.ProfilesChangedExternally += (_, _) => _ = Dispatcher.BeginInvoke(() => editor.RefreshCommand.Execute(null));
    }

    /// <summary>The Browse and Save As commands for the zip path, or null when the host has no file pickers.</summary>
    public ProfileZipBrowser? ZipBrowser { get; }

    /// <summary>The generated connection form over the editor.</summary>
    public FormViewModel Form { get; }

    private void ProfilesList_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        _editor.SelectedProfileNames.Clear();
        foreach (var name in ((ListBox)sender).SelectedItems.OfType<string>())
        {
            _editor.SelectedProfileNames.Add(name);
        }
    }
}
