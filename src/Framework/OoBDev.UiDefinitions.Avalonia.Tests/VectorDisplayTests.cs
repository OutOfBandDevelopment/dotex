using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.UiDefinitions.Avalonia;
using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.UiDefinitions.Avalonia.Tests;

/// <summary>The vector display drawn headless: the point follows the view model and the trail grows.</summary>
[TestCategory("Unit")]
[TestClass]
public sealed class VectorDisplayTests
{
    [TestMethod]
    public void Vector_Apply_MovesThePointAndGrowsTheTrail()
    {
        HeadlessSession.Instance.Dispatch(() =>
        {
            // Stage
            var model = new LiveDisplayViewModel(new VectorControl { Id = "v", Label = "Vector", XId = "x", YId = "y", Range = 10 });
            var window = new Window { Content = LiveDisplayView.Create(model) };
            window.Show();
            Dispatcher.UIThread.RunJobs();
            var dot = window.GetVisualDescendants().OfType<Ellipse>().Single();
            var trail = window.GetVisualDescendants().OfType<Polyline>().Single();

            // Test
            model.ApplyAll([new KeyValuePair<string, string>("x", "5"), new KeyValuePair<string, string>("y", "5")]);
            model.ApplyAll([new KeyValuePair<string, string>("x", "10"), new KeyValuePair<string, string>("y", "0")]);
            Dispatcher.UIThread.RunJobs();

            // Assert
            Assert.AreEqual(model.PlotX - 2.5, Canvas.GetLeft(dot), 1e-9);
            Assert.AreEqual(model.PlotY - 2.5, Canvas.GetTop(dot), 1e-9);
            Assert.IsGreaterThan(1, trail.Points!.Count);
            Assert.IsFalse(string.IsNullOrEmpty(model.Readout));
        }, default).GetAwaiter().GetResult();
    }
}
