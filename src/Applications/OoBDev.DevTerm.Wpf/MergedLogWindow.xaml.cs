using System;
using System.Windows;
using OoBDev.DevTerm.Wpf;

namespace DevTermApp;

/// <summary>Hosts a <see cref="MergedLogDialogViewModel"/>.</summary>
public partial class MergedLogWindow : Window
{
    /// <summary>Creates the window over <paramref name="log"/>.</summary>
    /// <param name="log">The window view model.</param>
    public MergedLogWindow(MergedLogDialogViewModel log)
    {
        ArgumentNullException.ThrowIfNull(log);
        InitializeComponent();
        DataContext = log;
    }
}
