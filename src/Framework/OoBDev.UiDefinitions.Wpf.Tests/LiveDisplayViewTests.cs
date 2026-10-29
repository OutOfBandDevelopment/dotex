using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
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
    public void Apply_StripChart_DrawsPolyline()
    {
        // Stage
        var model = new LiveDisplayViewModel(new StripChartControl
        {
            Id = "chart",
            Label = "Chart",
            HistoryLength = 3,
            Minimum = 0,
            Maximum = 10,
            Channels = [new ChartChannel { Id = "a", Label = "A" }],
        });
        var view = Render(model);

        // Test
        model.Apply("a", "0");
        model.Apply("a", "10");
        view.UpdateLayout();

        // Assert
        Assert.AreEqual(2, Find<Polyline>(view).Single(p => p.Stroke is not null && p.Opacity == 1).Points.Count);
    }

    [STATestMethod]
    public void Apply_Vector_ShowsReadout()
    {
        // Stage
        var model = new LiveDisplayViewModel(new VectorControl { Id = "v", Label = "V", XId = "x", YId = "y" });
        var view = Render(model);

        // Test
        model.Apply("x", "1");
        view.UpdateLayout();

        // Assert
        Assert.IsTrue(Find<TextBlock>(view).Any(t => t.Text.StartsWith("x=1", System.StringComparison.Ordinal)));
    }

    [STATestMethod]
    public void Apply_Vector_PlotsPointAndTrail()
    {
        // Stage
        var model = new LiveDisplayViewModel(new VectorControl { Id = "v", Label = "V", XId = "x", YId = "y", Range = 2 });
        var view = Render(model);
        model.ApplyAll([new("x", "0"), new("y", "0")]);

        // Test
        model.ApplyAll([new("x", "2"), new("y", "0")]);
        view.UpdateLayout();

        // Assert
        var marker = Find<Ellipse>(view).Single();
        Assert.AreEqual(97.5, Canvas.GetLeft(marker), 0.001);
        Assert.AreEqual(47.5, Canvas.GetTop(marker), 0.001);
        Assert.HasCount(2, Find<Polyline>(view).Single(p => p.Opacity < 1).Points);
    }

    [STATestMethod]
    public void Convert_NonNumber_ReturnsZero()
    {
        // Test
        var result = new CentreConverter().Convert("x", typeof(double), null!, System.Globalization.CultureInfo.InvariantCulture);

        // Assert
        Assert.AreEqual(0d, result);
    }

    [STATestMethod]
    public void ConvertBack_Always_Throws()
    {
        // Assert
        Assert.ThrowsExactly<System.NotSupportedException>(() => new CentreConverter().ConvertBack(1d, typeof(double), null!, System.Globalization.CultureInfo.InvariantCulture));
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
