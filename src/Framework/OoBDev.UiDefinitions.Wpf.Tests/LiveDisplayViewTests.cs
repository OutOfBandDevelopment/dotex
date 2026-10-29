using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.UiDefinitions.Wpf.Tests;

/// <summary>The live display view against real view models.</summary>
[TestCategory("Unit")]
[TestClass]
public sealed class LiveDisplayViewTests
{
    private static LiveDisplayView Render(LiveDisplayViewModel model)
    {
        var view = new LiveDisplayView { DataContext = model };
        view.Measure(new Size(500, 300));
        view.Arrange(new Rect(0, 0, 500, 300));
        view.UpdateLayout();
        return view;
    }

    private static T[] Find<T>(DependencyObject root)
        where T : DependencyObject
    {
        var found = new System.Collections.Generic.List<T>();
        for (var i = 0; i < VisualTreeHelper.GetChildrenCount(root); i++)
        {
            var child = VisualTreeHelper.GetChild(root, i);
            if (child is T match)
            {
                found.Add(match);
            }

            found.AddRange(Find<T>(child));
        }

        return [.. found];
    }

    [STATestMethod]
    public void Apply_BarGraph_MovesProgressBar()
    {
        // Stage
        var model = new LiveDisplayViewModel(new BarGraphControl
        {
            Id = "bars",
            Label = "Bars",
            Channels = [new ChartChannel { Id = "a", Label = "A" }],
        });
        var view = Render(model);

        // Test
        model.Apply("a", "40");
        view.UpdateLayout();

        // Assert
        Assert.AreEqual(0.4, Find<ProgressBar>(view).Single().Value, 1e-9);
    }

    [STATestMethod]
    public void Apply_Indicator_ShowsText()
    {
        // Stage
        var model = new LiveDisplayViewModel(new IndicatorControl { Id = "i", Label = "I", Expression = "a" });
        var view = Render(model);

        // Test
        model.Apply("a", "7");
        view.UpdateLayout();

        // Assert
        Assert.IsTrue(Find<TextBlock>(view).Any(t => t.Text == "7"));
    }
}
