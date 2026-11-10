using System.Windows;
using OoBDev.DevTerm.Wpf;

namespace DevTermApp;

/// <summary>The shell window; its <c>DataContext</c> is a <see cref="ShellViewModel"/>.</summary>
public partial class MainWindow : Window
{
    /// <summary>Creates the window.</summary>
    public MainWindow()
    {
        InitializeComponent();
        WpfTheme.Attach(this);
        if (LayoutFile is not null)
        {
            Loaded += (_, _) => DockLayout.Load(Dock, LayoutFile);
            Closing += (_, _) => DockLayout.Save(Dock, LayoutFile);
        }
    }

    /// <summary>Where the dock layout is saved as XML and restored from; null (the default, and in tests) keeps it unsaved.</summary>
    public static string? LayoutFile { get; set; }
}
