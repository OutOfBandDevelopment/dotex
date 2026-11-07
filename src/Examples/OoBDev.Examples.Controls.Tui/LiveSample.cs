using System;
using System.Collections.Generic;
using OoBDev.UiDefinitions;
using OoBDev.UiDefinitions.Tui;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;

namespace OoBDev.Examples.Controls.Tui;

/// <summary>The live displays (indicator, bar graph, strip chart) as Terminal.Gui views, fed by <see cref="LiveSampleFeed"/>: charts through <see cref="CellCharts"/> and <see cref="CellCanvasView"/>, the indicator as a label.</summary>
public sealed class LiveSample
{
    private readonly LiveSampleFeed _feed = new();
    private readonly List<CellCanvasView> _charts = [];
    private readonly List<(IndicatorState State, Label Label)> _indicators = [];

    /// <summary>Creates the displays and the view that holds them.</summary>
    public LiveSample()
    {
        Root = new View { Width = Dim.Fill(), Height = 1 };
        var y = 0;
        Root.Add(new Label { Y = y++, Text = "Live displays" });
        foreach (var control in LiveSampleFeed.Controls)
        {
            var state = LiveDisplayState.For(control)!;
            Root.Add(new Label { Y = y++, Text = control.Label ?? control.Id });
            switch (state)
            {
                case IndicatorState indicator:
                    var label = new Label { Y = y++, Text = string.Empty, Width = Dim.Fill() };
                    Root.Add(label);
                    _indicators.Add((indicator, label));
                    break;
                case BarGraphState bars:
                    y = AddChart(state, () => CellCharts.RenderBarGraph(bars), y);
                    break;
                case StripChartState strip:
                    y = AddChart(state, () => CellCharts.RenderStripChart(strip), y);
                    break;
            }
        }

        Root.Height = y;
    }

    /// <summary>The view to place in a window.</summary>
    public View Root { get; }

    /// <summary>The chart views, in display order.</summary>
    public IReadOnlyList<CellCanvasView> Charts => _charts;

    /// <summary>The indicator labels, in display order.</summary>
    public IReadOnlyList<Label> IndicatorLabels => [.. _indicators.ConvertAll(i => i.Label)];

    /// <summary>Publishes the next simulated sample to every display.</summary>
    public void Step()
    {
        var batch = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var pair in _feed.Next())
        {
            batch[pair.Key] = pair.Value;
        }

        foreach (var chart in _charts)
        {
            chart.Apply(batch);
        }

        foreach (var (state, label) in _indicators)
        {
            if (state.ApplyAll(batch) && state.Text is { } text)
            {
                label.Text = text;
            }
        }
    }

    private int AddChart(LiveDisplayState state, Func<CellGrid> render, int y)
    {
        var canvas = new CellCanvasView(state, render) { Y = y };
        Root.Add(canvas);
        _charts.Add(canvas);
        return y + render().Height;
    }
}
