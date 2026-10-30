using System;
using System.Windows;
using OoBDev.DevTerm.Wpf;

namespace DevTermApp;

/// <summary>Hosts a <see cref="ConfigureDeviceDialogViewModel"/>.</summary>
public partial class ConfigureDeviceWindow : Window
{
    /// <summary>Creates the window over <paramref name="device"/>.</summary>
    /// <param name="device">The view model it edits.</param>
    public ConfigureDeviceWindow(ConfigureDeviceDialogViewModel device)
    {
        ArgumentNullException.ThrowIfNull(device);
        InitializeComponent();
        WpfTheme.Attach(this);
        DataContext = device;
    }
}
