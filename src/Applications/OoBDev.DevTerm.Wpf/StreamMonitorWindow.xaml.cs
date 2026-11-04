using System;
using System.Windows;
using OoBDev.DevTerm.Wpf;

namespace DevTermApp;

/// <summary>Hosts a <see cref="StreamMonitorDialogViewModel"/>.</summary>
public partial class StreamMonitorWindow : Window
{
    /// <summary>Creates the window over <paramref name="monitor"/>.</summary>
    /// <param name="monitor">The window view model.</param>
    public StreamMonitorWindow(StreamMonitorDialogViewModel monitor)
    {
        ArgumentNullException.ThrowIfNull(monitor);
        InitializeComponent();
        WpfTheme.Attach(this);
        DataContext = monitor;
    }
}
