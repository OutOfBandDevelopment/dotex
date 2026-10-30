using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.DevTerm.Wpf.Tests;

/// <summary>The stream monitor window opened from the shell.</summary>
[TestCategory("Unit")]
[TestClass]
public sealed class StreamMonitorDialogTests
{
    private sealed class FakeStreamMonitorDialog(Action<StreamMonitorDialogViewModel> interact) : IStreamMonitorDialog
    {
        public void Show(StreamMonitorDialogViewModel monitor) => interact(monitor);
    }

    [TestMethod]
    public async Task ShowStreamMonitorWindow_SelectedSession_StartAndStopTogglesRunning()
    {
        // Stage
        bool? runningAfterStart = null;
        bool? runningAfterStop = null;
        var services = new ServiceCollection();
        services.AddSingleton<IStreamMonitorDialog>(new FakeStreamMonitorDialog(monitor =>
        {
            monitor.Start.Execute(null);
            runningAfterStart = monitor.IsRunning;
            monitor.Stop.Execute(null);
            runningAfterStop = monitor.IsRunning;
        }));
        await using var provider = services.AddDevTermWpfShell().BuildServiceProvider();
        var shell = provider.GetRequiredService<ShellViewModel>();
        shell.NewLoopbackSession.Execute(null);

        // Test
        shell.ShowStreamMonitorWindow.Execute(null);

        // Assert
        Assert.IsTrue(runningAfterStart);
        Assert.IsFalse(runningAfterStop);
    }

    [TestMethod]
    public async Task ShowStreamMonitorWindow_NoSession_CannotExecute()
    {
        // Stage
        var services = new ServiceCollection();
        services.AddSingleton<IStreamMonitorDialog>(new FakeStreamMonitorDialog(_ => { }));
        await using var provider = services.AddDevTermWpfShell().BuildServiceProvider();
        var shell = provider.GetRequiredService<ShellViewModel>();

        // Assert
        Assert.IsFalse(shell.ShowStreamMonitorWindow.CanExecute(null));
    }
}
