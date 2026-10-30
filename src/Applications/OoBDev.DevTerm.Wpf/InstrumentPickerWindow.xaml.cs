using System;
using System.Windows;
using System.Windows.Input;
using OoBDev.DevTerm.Wpf;

namespace DevTermApp;

/// <summary>Hosts an <see cref="InstrumentPickerDialogViewModel"/>.</summary>
public partial class InstrumentPickerWindow : Window
{
    private readonly InstrumentPickerDialogViewModel _picker;

    /// <summary>Creates the window over <paramref name="picker"/>.</summary>
    /// <param name="picker">The picker view model.</param>
    public InstrumentPickerWindow(InstrumentPickerDialogViewModel picker)
    {
        ArgumentNullException.ThrowIfNull(picker);
        _picker = picker;
        InitializeComponent();
        WpfTheme.Attach(this);
        picker.SelectedIndex = 0;
        DataContext = picker;
    }

    private void Commit()
    {
        if (_picker.TryChoose())
        {
            DialogResult = true;
        }
    }

    private void Select_Click(object sender, RoutedEventArgs e) => Commit();

    private void List_MouseDoubleClick(object sender, MouseButtonEventArgs e) => Commit();
}
