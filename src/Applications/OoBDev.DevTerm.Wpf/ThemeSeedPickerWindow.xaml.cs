using System;
using System.Windows;
using System.Windows.Input;
using OoBDev.DevTerm.Wpf;

namespace DevTermApp;

/// <summary>Hosts a <see cref="ThemeSeedPickerViewModel"/>.</summary>
public partial class ThemeSeedPickerWindow : Window
{
    private readonly ThemeSeedPickerViewModel _picker;

    /// <summary>Creates the window over <paramref name="picker"/>.</summary>
    /// <param name="picker">The seed picker view model.</param>
    public ThemeSeedPickerWindow(ThemeSeedPickerViewModel picker)
    {
        ArgumentNullException.ThrowIfNull(picker);
        _picker = picker;
        InitializeComponent();
        WpfTheme.Attach(this);
        DataContext = picker;
    }

    private void Commit()
    {
        if (_picker.TryChoose())
        {
            DialogResult = true;
        }
    }

    private void Create_Click(object sender, RoutedEventArgs e) => Commit();

    private void List_MouseDoubleClick(object sender, MouseButtonEventArgs e) => Commit();
}
