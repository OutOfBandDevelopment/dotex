using System;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Threading;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OoBDev.Examples.Controls.Wpf.Tests;

/// <summary>The live displays on real WPF views; needs an STA thread and nothing is shown.</summary>
[TestCategory("Unit")]
[TestClass]
public sealed class LiveSampleTests
{
    [TestMethod]
    public void Constructor_CreatesOneDisplayPerFeedControl()
    {
        // Test
        var ids = Run(() => new LiveSample().Displays.Select(d => d.Control.Id).ToList());

        // Assert
        Assert.AreEqual("sum,bars,strip", string.Join(",", ids));
    }

    [TestMethod]
    public void Step_PublishesTheFeed_ToEveryDisplay()
    {
        // Test
        var revisions = Run(() =>
        {
            var live = new LiveSample();
            var before = live.Displays.Select(d => d.Revision).ToList();
            live.Step();
            live.Step();
            return live.Displays.Select((d, i) => d.Revision - before[i]).ToList();
        });

        // Assert
        Assert.IsTrue(revisions.All(r => r > 0), string.Join(",", revisions));
    }

    [TestMethod]
    public void Build_PlacesTheLivePanelInTheWindow()
    {
        // Test
        var count = Run(() =>
        {
            var window = ControlsWindow.Build(new ControlsSampleSettings(), out _, out var live);
            return ((global::System.Windows.Controls.Grid)window.Content).Children.Contains(live.Element) ? 1 : 0;
        });

        // Assert
        Assert.AreEqual(1, count);
    }

    private static T Run<T>(Func<T> action)
    {
        T result = default!;
        Exception? error = null;
        var thread = new Thread(() =>
        {
            try
            {
                result = action();
            }
            catch (Exception ex)
            {
                error = ex;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();
        if (error is not null)
        {
            ExceptionDispatchInfo.Capture(error).Throw();
        }

        return result;
    }
}
