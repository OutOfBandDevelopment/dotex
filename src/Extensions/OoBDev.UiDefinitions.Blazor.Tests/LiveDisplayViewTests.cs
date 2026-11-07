using System.Collections.Generic;
using Bunit;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.UiDefinitions;
using OoBDev.UiDefinitions.Blazor;

namespace OoBDev.UiDefinitions.Blazor.Tests;

/// <summary><see cref="LiveDisplayView"/> draws indicators, bar graphs and strip charts from their state and redraws on <c>Refresh</c>.</summary>
[TestCategory("Unit")]
[TestClass]
public sealed class LiveDisplayViewTests
{
    private static IRenderedComponent<LiveDisplayView> Render(Bunit.TestContext context, LiveDisplayState state) =>
        context.RenderComponent<LiveDisplayView>(p => p.Add(c => c.State, state));

    [TestMethod]
    public void Render_Indicator_ShowsDefaultThenDerivedText()
    {
        // Test
        using var context = new Bunit.TestContext();
        var control = new IndicatorControl { Id = "total", Label = "Total", DefaultValue = "none", Expression = "a + b" };
        var state = (IndicatorState)LiveDisplayState.For(control)!;
        var cut = Render(context, state);
        var before = cut.Find("[data-indicator=total]").TextContent;
        state.ApplyAll([new KeyValuePair<string, string>("a", "1"), new KeyValuePair<string, string>("b", "2")]);
        cut.Instance.Refresh();

        // Assert
        Assert.AreEqual("none", before);
        Assert.AreEqual("3", cut.Find("[data-indicator=total]").TextContent);
    }

    [TestMethod]
    public void Render_BarGraph_FillsEachBarByItsFraction()
    {
        // Test
        using var context = new Bunit.TestContext();
        var control = new BarGraphControl
        {
            Id = "levels",
            Label = "Levels",
            Minimum = 0,
            Maximum = 10,
            Channels = [new ChartChannel { Id = "x", Label = "X" }, new ChartChannel { Id = "y" }],
        };
        var state = (BarGraphState)LiveDisplayState.For(control)!;
        state.Apply("x", "5");
        var cut = Render(context, state);

        // Assert
        Assert.AreEqual("50", cut.Find("[data-channel=x] .uidef-bar-fill").GetAttribute("width"));
        Assert.AreEqual("0", cut.Find("[data-channel=y] .uidef-bar-fill").GetAttribute("width"));
        Assert.AreEqual("-", cut.Find("[data-channel=y] output").TextContent);
    }

    [TestMethod]
    public void Render_StripChart_DrawsOnePolylinePerChannelWithSamples_AndGrowsOnRefresh()
    {
        // Test
        using var context = new Bunit.TestContext();
        var control = new StripChartControl
        {
            Id = "trend",
            Label = "Trend",
            Minimum = 0,
            Maximum = 10,
            HistoryLength = 5,
            Channels = [new ChartChannel { Id = "a" }, new ChartChannel { Id = "b" }],
        };
        var state = (StripChartState)LiveDisplayState.For(control)!;
        state.Apply("a", "5");
        var cut = Render(context, state);
        var first = cut.FindAll("polyline").Count;
        state.Apply("a", "10");
        cut.Instance.Refresh();

        // Assert
        Assert.AreEqual(1, first);
        Assert.AreEqual(1, cut.FindAll("polyline").Count);
        Assert.AreEqual(2, cut.Find("polyline[data-channel=a]").GetAttribute("points")!.Split(' ').Length);
    }

    [TestMethod]
    public void Render_Vector_ShowsNoPointUntilItArrives_ThenPlotsItAndTheTrail()
    {
        // Test
        using var context = new Bunit.TestContext();
        var state = (VectorState)LiveDisplayState.For(new VectorControl { Id = "v", Label = "V", XId = "x", YId = "y", Range = 10 })!;
        var cut = Render(context, state);
        var before = cut.FindAll("circle.uidef-point").Count;
        state.ApplyAll([new KeyValuePair<string, string>("x", "0"), new KeyValuePair<string, string>("y", "0")]);
        state.ApplyAll([new KeyValuePair<string, string>("x", "5"), new KeyValuePair<string, string>("y", "-5")]);
        cut.Instance.Refresh();

        // Assert
        Assert.AreEqual(0, before);
        var point = cut.Find("circle.uidef-point");
        Assert.AreEqual("75", point.GetAttribute("cx"));
        Assert.AreEqual("75", point.GetAttribute("cy"));
        Assert.AreEqual("50,50", cut.Find("polyline.uidef-trail").GetAttribute("points"));
        Assert.AreEqual("x=5 y=-5", cut.Find("[data-readout=v]").TextContent);
    }
}
