using System;
using System.Windows;
using OoBDev.DevTerm.Wpf;

namespace DevTermApp;

/// <summary>Hosts a <see cref="RoutingDialogViewModel"/>.</summary>
public partial class RoutingWindow : Window
{
    /// <summary>Creates the window over <paramref name="routing"/>.</summary>
    /// <param name="routing">The routing window view model.</param>
    public RoutingWindow(RoutingDialogViewModel routing)
    {
        ArgumentNullException.ThrowIfNull(routing);
        InitializeComponent();
        DataContext = routing;
    }
}
