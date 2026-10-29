using System.Windows;

namespace OoBDev.DevTerm.Wpf;

/// <summary>The composition root: registers the panel factories and shows the shell.</summary>
public partial class App : Application
{
    /// <inheritdoc />
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        new MainWindow { DataContext = new ShellViewModel([new DemoPanelFactory()]) }.Show();
    }
}
