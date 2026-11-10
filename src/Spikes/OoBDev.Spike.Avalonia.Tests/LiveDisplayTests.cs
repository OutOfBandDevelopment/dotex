using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Headless;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.Spike.Avalonia;

namespace OoBDev.Spike.Avalonia.Tests;

/// <summary>The live displays (indicator, bar graph, strip chart) drawn headless from the shared sample feed, with screenshots attached.</summary>
[TestCategory("Unit")]
[TestClass]
public sealed class LiveDisplayTests
{
    private static HeadlessUnitTestSession _session => HeadlessSession.Instance;

    public TestContext TestContext { get; set; } = null!;

    private void Run(int steps, global::System.Action<LiveDemo, Window> body) => _session.Dispatch(() =>
    {
        var demo = new LiveDemo();
        for (var i = 0; i < steps; i++)
        {
            demo.Step();
        }

        var window = demo.CreateWindow(run: false);
        window.Show();
        Dispatcher.UIThread.RunJobs();
        body(demo, window);
    }, default).GetAwaiter().GetResult();

    [TestMethod]
    public void StripChart_AfterSamples_DrawsOneTracePerChannelWithTheSamples()
    {
        Run(30, (_, window) =>
        {
            var traces = window.GetVisualDescendants().OfType<Polyline>().Where(p => p.Points!.Count > 2).ToList();

            Assert.HasCount(2, traces);
            Assert.IsTrue(traces.All(t => t.Points!.Count == 30), "One point per published sample.");
            Assert.IsTrue(traces.All(t => t.Points!.All(p => p.Y is >= 0 and <= 100)), "Points stay inside the fixed 100 by 100 box.");
            Screenshots.Save(TestContext, window, "30-samples");
        });
    }

    [TestMethod]
    public void StripChart_MoreSamplesThanHistory_KeepsOnlyTheHistoryLength()
    {
        Run(90, (_, window) =>
        {
            var traces = window.GetVisualDescendants().OfType<Polyline>().Where(p => p.Points!.Count > 2).ToList();

            Assert.IsTrue(traces.All(t => t.Points!.Count == 60), "HistoryLength is 60.");
            Screenshots.Save(TestContext, window, "90-samples");
        });
    }

    [TestMethod]
    public void BarGraph_AfterSamples_ShowsTheLatestValueAsAFraction()
    {
        Run(12, (demo, window) =>
        {
            var bars = window.GetVisualDescendants().OfType<ProgressBar>().ToList();
            var model = demo.Displays.Single(d => d.IsBarGraph);

            Assert.HasCount(2, bars);
            Assert.AreEqual(model.Bars[0].Fraction, bars[0].Value, 1e-9);
            Assert.IsGreaterThan(0, bars[0].Value);
            Screenshots.Save(TestContext, window, "bars");
        });
    }

    [TestMethod]
    public void Indicator_AfterSamples_ShowsTheSumOfTheChannels()
    {
        Run(5, (demo, window) =>
        {
            var model = demo.Displays.Single(d => d.IsIndicator);
            var text = window.GetVisualDescendants().OfType<TextBlock>().Single(t => t.FontSize == 20);

            Assert.IsFalse(string.IsNullOrEmpty(model.Text));
            Assert.AreEqual(model.Text, text.Text);
        });
    }

    [TestMethod]
    public void StripChart_NewSampleAfterRender_RedrawsTheTrace()
    {
        Run(10, (demo, window) =>
        {
            var before = window.GetVisualDescendants().OfType<Polyline>().First(p => p.Points!.Count > 2).Points!.Count;

            demo.Step();
            Dispatcher.UIThread.RunJobs();

            var after = window.GetVisualDescendants().OfType<Polyline>().First(p => p.Points!.Count > 2).Points!.Count;
            Assert.AreEqual(before + 1, after);
        });
    }
}
