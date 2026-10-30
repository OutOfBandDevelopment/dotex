using System.Windows;

namespace DevTermApp;

/// <summary>The shell window; its <c>DataContext</c> is a <see cref="ShellViewModel"/>.</summary>
public partial class MainWindow : Window
{
    /// <summary>Creates the window.</summary>
    public MainWindow() => InitializeComponent();
}
