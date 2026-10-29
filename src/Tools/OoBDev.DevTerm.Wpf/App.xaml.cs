using System.Windows;
using OoBDev.DevTerm.Wpf;
using OoBDev.UiDefinitions.Wpf;

namespace DevTermApp;

/// <summary>The composition root: registers the panel factories and shows the shell.</summary>
public partial class App : Application
{
    /// <inheritdoc />
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        new MainWindow { DataContext = new ShellViewModel([new DemoPanelFactory(new WpfColorPicker())]) }.Show();
    }
}
