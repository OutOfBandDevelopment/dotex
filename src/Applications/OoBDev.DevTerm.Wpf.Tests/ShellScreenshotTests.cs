using System;
using System.IO;
using System.Threading;
using System.Windows.Threading;
using DevTermApp;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.DevTerm.Configuration;
using OoBDev.DevTerm.Wpf;

namespace OoBDev.DevTerm.Wpf.Tests;

/// <summary>The shell window over a loopback session, captured for the user guide.</summary>
[DoNotParallelize]
[TestCategory("DevLocal")]
[TestClass]
public sealed class ShellScreenshotTests
{
    [TestMethod]
    public void MainWindow_LoopbackSessionWithTraffic_IsCaptured() =>
        WpfScreenshot.Capture(BuildWindow, "wpf-main-window.png", 900, 650);

    internal static MainWindow BuildWindow()
    {
        SynchronizationContext.SetSynchronizationContext(new DispatcherSynchronizationContext(Dispatcher.CurrentDispatcher));
        var directory = Directory.CreateTempSubdirectory("devterm-shell-");
        var store = new ConnectionProfileStore(directory.FullName);
        store.Save("korad-ka3005p", new CliOptions { Transport = "serial", Port = "COM6" });
        store.Save("tek-2230", new CliOptions { Transport = "tcp", Host = "192.168.0.108", Port = "23" });
        var services = new ServiceCollection();
        services.AddSingleton(store);
        var provider = services.AddDevTermWpfShell().BuildServiceProvider();
        var shell = provider.GetRequiredService<ShellViewModel>();
        shell.NewLoopbackSession.Execute(null);
        var session = shell.SelectedSession!;
        session.Connect.Execute(null);
        session.SendText = "*IDN?";
        session.Send.Execute(null);
        for (var i = 0; i < 100 && session.OutputLines.Count == 0; i++)
        {
            Thread.Sleep(20);
            Dispatcher.CurrentDispatcher.Invoke(() => { }, DispatcherPriority.Background);
        }

        return new MainWindow { DataContext = shell };
    }
}
