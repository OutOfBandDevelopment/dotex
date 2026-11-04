using System;
using System.Windows;
using Microsoft.Win32;
using OoBDev.DevTerm.Wpf;

namespace DevTermApp;

/// <summary>Hosts a <see cref="ConverterToolsDialogViewModel"/>.</summary>
public partial class ConverterToolsWindow : Window
{
    private readonly ConverterToolsDialogViewModel _tools;

    /// <summary>Creates the window over <paramref name="tools"/>.</summary>
    /// <param name="tools">The converter tools window view model.</param>
    public ConverterToolsWindow(ConverterToolsDialogViewModel tools)
    {
        ArgumentNullException.ThrowIfNull(tools);
        _tools = tools;
        InitializeComponent();
        WpfTheme.Attach(this);
        DataContext = tools;
    }

    private void Browse_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog { Title = "Converter executable", Filter = "Programs (*.exe;*.bat;*.cmd)|*.exe;*.bat;*.cmd|All files (*.*)|*.*" };
        if (dialog.ShowDialog(this) == true)
        {
            _tools.Path = dialog.FileName;
        }
    }

    private void Ok_Click(object sender, RoutedEventArgs e)
    {
        if (_tools.TryAccept())
        {
            DialogResult = true;
        }
    }
}
