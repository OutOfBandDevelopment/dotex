using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;
using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.UiDefinitions.WinForms;

/// <summary>
/// Draws a <see cref="LiveDisplayViewModel"/> (indicator, bar graph, strip chart or vector) with Windows Forms controls, and redraws
/// when the view model publishes a change. Publish from the UI thread.
/// </summary>
public sealed class LiveDisplayView : UserControl
{
    private readonly LiveDisplayViewModel _model;
    private readonly List<Action> _unhook = [];

    /// <summary>Builds the controls for <paramref name="model"/>.</summary>
    public LiveDisplayView(LiveDisplayViewModel model)
    {
        _model = model ?? throw new ArgumentNullException(nameof(model));
        AutoSize = true;
        var stack = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.TopDown, WrapContents = false, Dock = DockStyle.Fill };
        stack.Controls.Add(new Label { Text = model.Label, AutoSize = true, Font = new Font(Font, FontStyle.Bold) });
        if (model.IsIndicator)
        {
            Indicator = new Label { AutoSize = true, Font = new Font(Font.FontFamily, 14f), Text = model.Text };
            Follow(model, () => Indicator.Text = model.Text);
            stack.Controls.Add(Indicator);
        }
        else if (model.IsBarGraph)
        {
            foreach (var bar in model.Bars)
            {
                stack.Controls.Add(Bar(bar));
            }
        }
        else if (model.IsStripChart)
        {
            stack.Controls.Add(Chart(model));
        }
        else if (model.IsVector)
        {
            stack.Controls.Add(Vector(model));
        }

        Controls.Add(stack);
    }

    /// <summary>The text label of an indicator display, otherwise null.</summary>
    public Label? Indicator { get; }

    /// <summary>The progress bars of a bar graph, in channel order.</summary>
    public IReadOnlyList<ProgressBar> Bars => _bars;

    /// <summary>The plot of a strip chart or vector display, otherwise null.</summary>
    public PlotSurface? Plot { get; private set; }

    private readonly List<ProgressBar> _bars = [];

    /// <summary>Parses the <c>x,y x,y</c> text a view model publishes into points.</summary>
    public static PointF[] ParsePoints(string text) => [.. text
        .Split(' ', StringSplitOptions.RemoveEmptyEntries)
        .Select(pair => pair.Split(','))
        .Where(parts => parts.Length == 2)
        .Select(parts => new PointF(float.Parse(parts[0], CultureInfo.InvariantCulture), float.Parse(parts[1], CultureInfo.InvariantCulture)))];

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            foreach (var undo in _unhook)
            {
                undo();
            }
        }

        base.Dispose(disposing);
    }

    private TableLayoutPanel Bar(BarViewModel bar)
    {
        var row = new TableLayoutPanel { AutoSize = true, ColumnCount = 3 };
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 120));
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 220));
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90));
        var progress = new ProgressBar { Minimum = 0, Maximum = 1000, Width = 210, Height = 16 };
        var value = new Label { AutoSize = true };
        _bars.Add(progress);
        row.Controls.Add(new Label { Text = bar.Label, AutoSize = true, ForeColor = ColorTranslator.FromHtml(bar.Color) });
        row.Controls.Add(progress);
        row.Controls.Add(value);
        Follow(bar, () =>
        {
            progress.Value = Math.Clamp((int)Math.Round(bar.Fraction * 1000), 0, 1000);
            value.Text = bar.ValueText;
        });
        return row;
    }

    private FlowLayoutPanel Chart(LiveDisplayViewModel model)
    {
        var plot = new PlotSurface { Width = 360, Height = 120 };
        Plot = plot;
        var legend = new FlowLayoutPanel { AutoSize = true };
        foreach (var line in model.Lines)
        {
            legend.Controls.Add(new Label { Text = "■ " + line.Label, AutoSize = true, ForeColor = ColorTranslator.FromHtml(line.Color) });
        }

        void Redraw()
        {
            plot.Traces.Clear();
            foreach (var line in model.Lines)
            {
                plot.Traces.Add((ColorTranslator.FromHtml(line.Color), ParsePoints(line.Points)));
            }

            plot.Invalidate();
        }

        foreach (var line in model.Lines)
        {
            Follow(line, Redraw);
        }

        Redraw();
        return Column(plot, legend);
    }

    private FlowLayoutPanel Vector(LiveDisplayViewModel model)
    {
        var plot = new PlotSurface { Width = 160, Height = 160, Crosshair = true };
        Plot = plot;
        var readout = new Label { AutoSize = true, Font = new Font(FontFamily.GenericMonospace, Font.Size) };
        Follow(model, () =>
        {
            var color = ColorTranslator.FromHtml(model.PlotColor);
            plot.Traces.Clear();
            plot.Traces.Add((color, ParsePoints(model.PlotTrail)));
            plot.Marker = new PointF((float)model.PlotX, (float)model.PlotY);
            plot.MarkerColor = color;
            readout.Text = model.Readout;
            plot.Invalidate();
        });
        return Column(plot, readout);
    }

    private static FlowLayoutPanel Column(params Control[] controls)
    {
        var column = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.TopDown, WrapContents = false };
        column.Controls.AddRange(controls);
        return column;
    }

    private void Follow(global::System.ComponentModel.INotifyPropertyChanged source, Action apply)
    {
        void Handler(object? sender, global::System.ComponentModel.PropertyChangedEventArgs e) => apply();
        source.PropertyChanged += Handler;
        _unhook.Add(() => source.PropertyChanged -= Handler);
        apply();
    }
}
