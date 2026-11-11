using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.UiDefinitions.WinForms.Tests;

/// <summary>The Windows Forms live displays against real view models.</summary>
[TestCategory("Unit")]
[STATestClass]
public sealed class LiveDisplayViewTests
{
    private static readonly ChartChannel[] _channels =
    [
        new ChartChannel { Id = "a", Label = "A" },
        new ChartChannel { Id = "b", Label = "B" },
    ];

    private static KeyValuePair<string, string>[] Sample(string a, string b) => [new("a", a), new("b", b)];

    [TestMethod]
    public void Indicator_Apply_UpdatesTheText()
    {
        // Stage
        var model = new LiveDisplayViewModel(new IndicatorControl { Id = "i", Label = "Sum", Expression = "a + b", Unit = "V" });
        using var view = new LiveDisplayView(model);

        // Test
        model.ApplyAll(Sample("1", "2"));

        // Assert
        Assert.IsNotNull(view.Indicator);
        Assert.AreEqual(model.Text, view.Indicator.Text);
        Assert.Contains("3", view.Indicator.Text, global::System.StringComparison.Ordinal);
    }

    [TestMethod]
    public void BarGraph_Apply_MovesEachBarToItsFraction()
    {
        // Stage
        var model = new LiveDisplayViewModel(new BarGraphControl { Id = "g", Label = "Bars", Minimum = 0, Maximum = 10, Channels = [.. _channels] });
        using var view = new LiveDisplayView(model);

        // Test
        model.ApplyAll(Sample("10", "5"));

        // Assert
        Assert.HasCount(2, view.Bars);
        Assert.AreEqual(1000, view.Bars[0].Value);
        Assert.AreEqual(500, view.Bars[1].Value);
    }

    [TestMethod]
    public void StripChart_Apply_PublishesATracePerChannel()
    {
        // Stage
        var model = new LiveDisplayViewModel(new StripChartControl { Id = "s", Label = "Trend", Minimum = 0, Maximum = 10, HistoryLength = 10, Channels = [.. _channels] });
        using var view = new LiveDisplayView(model);

        // Test
        model.ApplyAll(Sample("1", "2"));
        model.ApplyAll(Sample("3", "4"));

        // Assert
        Assert.IsNotNull(view.Plot);
        Assert.HasCount(2, view.Plot.Traces);
        Assert.IsTrue(view.Plot.Traces.All(t => t.Points.Length == 2));
    }

    [TestMethod]
    public void ParsePoints_PairText_ReturnsInvariantPoints()
    {
        // Test
        var points = LiveDisplayView.ParsePoints("1.5,2 3,4.25 bad");

        // Assert
        Assert.HasCount(2, points);
        Assert.AreEqual(1.5f, points[0].X);
        Assert.AreEqual(4.25f, points[1].Y);
    }

    [TestMethod]
    public void Plot_Paint_DrawsWithoutError()
    {
        // Stage
        using var plot = new PlotSurface { Width = 100, Height = 100, Crosshair = true, Marker = new global::System.Drawing.PointF(50, 50) };
        plot.Traces.Add((global::System.Drawing.Color.Red, [new global::System.Drawing.PointF(0, 0), new global::System.Drawing.PointF(100, 100)]));
        using var bitmap = new global::System.Drawing.Bitmap(100, 100);

        // Test
        plot.DrawToBitmap(bitmap, new global::System.Drawing.Rectangle(0, 0, 100, 100));

        // Assert
        Assert.AreNotEqual(global::System.Drawing.Color.Empty, bitmap.GetPixel(50, 50));
    }
}
