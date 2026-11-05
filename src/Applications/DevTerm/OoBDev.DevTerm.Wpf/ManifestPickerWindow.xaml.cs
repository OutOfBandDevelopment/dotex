using System;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using OoBDev.DevTerm.Wpf;

namespace DevTermApp;

/// <summary>Hosts a <see cref="ManifestPickerDialogViewModel"/>.</summary>
public partial class ManifestPickerWindow : Window
{
    private readonly ManifestPickerDialogViewModel _picker;

    /// <summary>Creates the window over <paramref name="picker"/>.</summary>
    /// <param name="picker">The picker view model.</param>
    public ManifestPickerWindow(ManifestPickerDialogViewModel picker)
    {
        ArgumentNullException.ThrowIfNull(picker);
        _picker = picker;
        InitializeComponent();
        WpfTheme.Attach(this);
        DataContext = picker;
    }

    private void Commit()
    {
        if (_picker.TryLoad())
        {
            DialogResult = true;
        }
    }

    private void Open_Click(object sender, RoutedEventArgs e) => Commit();

    private void List_MouseDoubleClick(object sender, MouseButtonEventArgs e) => Commit();

    private void BrowseFile_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog { Filter = "Device manifests (*.json;*.zip)|*.json;*.zip|All files (*.*)|*.*" };
        if (dialog.ShowDialog(this) == true)
        {
            _picker.PathText = dialog.FileName;
        }
    }

    private void BrowseFolder_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog { Title = "Folder containing device.json" };
        if (dialog.ShowDialog(this) == true)
        {
            _picker.PathText = dialog.FolderName;
        }
    }
}
