using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.System.IO;

namespace OoBDev.DevTerm.Wpf.Tests;

/// <summary>The merged log window opened from the shell.</summary>
[TestCategory("Unit")]
[TestClass]
public sealed class MergedLogDialogTests
{
    private sealed class FakeMergedLogDialog(Action<MergedLogDialogViewModel> interact) : IMergedLogDialog
    {
        public void Show(MergedLogDialogViewModel log)
        {
            using (log)
            {
                interact(log);
            }
        }
    }

    [TestMethod]
    public async Task ShowMergedLogWindow_SessionSendsText_ListsTheSentLineAndClearEmptiesIt()
    {
        // Stage
        var seen = new List<string>();
        var afterClear = -1;
        var services = new ServiceCollection();
        services.AddSingleton<IMergedLogDialog>(new FakeMergedLogDialog(log =>
        {
            seen.AddRange(log.Lines);
            log.Clear.Execute(null);
            afterClear = log.Lines.Count;
        }));
        await using var provider = services.AddDevTermWpfShell().BuildServiceProvider();
        var shell = provider.GetRequiredService<ShellViewModel>();
        shell.NewLoopbackSession.Execute(null);
        var session = shell.SelectedSession!;
        session.Connect.Execute(null);
        await WaitAsync(() => session.State == ConnectionState.Open);
        session.SendText = "hello";
        session.Send.Execute(null);
        await WaitAsync(() => session.OutputLines.Count > 0);

        // Test
        shell.ShowMergedLogWindow.Execute(null);

        // Assert
        Assert.IsTrue(seen.Exists(line => line.Contains("hello", StringComparison.Ordinal)));
        Assert.AreEqual(0, afterClear);
    }

    [TestMethod]
    public void ShowMergedLogWindow_NoLogOrDialog_CannotExecute()
    {
        // Stage
        using var shell = new ShellViewModel([]);

        // Assert
        Assert.IsFalse(shell.ShowMergedLogWindow.CanExecute(null));
    }

    private static async Task WaitAsync(Func<bool> condition)
    {
        var watch = Stopwatch.StartNew();
        while (!condition())
        {
            Assert.IsLessThan(5000, watch.ElapsedMilliseconds, "Timed out waiting for the condition.");
            await Task.Delay(10);
        }
    }
}
