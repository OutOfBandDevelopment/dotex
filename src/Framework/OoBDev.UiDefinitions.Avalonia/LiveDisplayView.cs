using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Layout;
using Avalonia.Media;
using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.UiDefinitions.Avalonia;

/// <summary>
/// Draws a <see cref="LiveDisplayViewModel"/> (indicator, bar graph, strip chart or vector) in Avalonia. The strip chart and vector are
/// drawn in a fixed 100 by 100 box scaled by a <see cref="Viewbox"/>, so the control's configured axis is honoured exactly.
/// </summary>
public static class LiveDisplayView
{
    /// <summary>Creates the view; it redraws itself when the view model publishes a change.</summary>
    public static Control Create(LiveDisplayViewModel model)
    {
        ArgumentNullException.ThrowIfNull(model);
        var panel = new StackPanel { Spacing = 2 };
        panel.Children.Add(new TextBlock { Text = model.Label, FontWeight = FontWeight.Bold });

        if (model.IsIndicator)
        {
            var text = new TextBlock { FontSize = 20 };
            text.Bind(TextBlock.TextProperty, new global::Avalonia.Data.Binding(nameof(LiveDisplayViewModel.Text)) { Source = model });
            panel.Children.Add(text);
        }
        else if (model.IsBarGraph)
        {
            foreach (var bar in model.Bars)
            {
                panel.Children.Add(Bar(bar));
            }
        }
        else if (model.IsStripChart)
        {
            panel.Children.Add(Chart(model));
        }
        else if (model.IsVector)
        {
            panel.Children.Add(Vector(model));
        }

        return panel;
    }

    /// <summary>Parses the <c>x,y x,y</c> text a view model publishes into points.</summary>
    public static List<Point> ParsePoints(string text) => [.. text
        .Split(' ', StringSplitOptions.RemoveEmptyEntries)
        .Select(pair => pair.Split(','))
        .Where(parts => parts.Length == 2)
        .Select(parts => new Point(double.Parse(parts[0], CultureInfo.InvariantCulture), double.Parse(parts[1], CultureInfo.InvariantCulture)))];

    private static Control Bar(BarViewModel bar)
    {
        var grid = new Grid { ColumnDefinitions = new ColumnDefinitions("120,*,90"), Margin = new Thickness(0, 2) };
        grid.Children.Add(new TextBlock { Text = bar.Label, VerticalAlignment = VerticalAlignment.Center });

        var progress = new ProgressBar { Minimum = 0, Maximum = 1, Height = 14, Foreground = Brush.Parse(bar.Color), Value = bar.Fraction };
        Grid.SetColumn(progress, 1);
        grid.Children.Add(progress);

        var value = new TextBlock { Text = bar.ValueText, Margin = new Thickness(8, 0, 0, 0), VerticalAlignment = VerticalAlignment.Center };
        Grid.SetColumn(value, 2);
        grid.Children.Add(value);

        bar.PropertyChanged += (_, _) =>
        {
            progress.Value = bar.Fraction;
            value.Text = bar.ValueText;
        };
        return grid;
    }

    private static Control Chart(LiveDisplayViewModel model)
    {
        var canvas = new Canvas { Width = 100, Height = 100 };
        canvas.Children.Add(new Rectangle { Width = 100, Height = 100, Stroke = Brushes.Gray, StrokeThickness = 0.5 });
        foreach (var y in new[] { 25d, 50d, 75d })
        {
            canvas.Children.Add(new Line { StartPoint = new Point(0, y), EndPoint = new Point(100, y), Stroke = Brushes.LightGray, StrokeThickness = 0.3 });
        }

        var legend = new StackPanel { Orientation = Orientation.Horizontal, Spacing = 12 };
        foreach (var line in model.Lines)
        {
            var polyline = new Polyline { Stroke = Brush.Parse(line.Color), StrokeThickness = 1.2, Points = ParsePoints(line.Points) };
            canvas.Children.Add(polyline);
            line.PropertyChanged += (_, e) => Redraw(line, polyline, e);
            legend.Children.Add(new TextBlock { Text = "■ " + line.Label, Foreground = Brush.Parse(line.Color) });
        }

        var box = new Viewbox { Width = 360, Height = 120, Stretch = Stretch.Fill, HorizontalAlignment = HorizontalAlignment.Left, Child = canvas };
        return new StackPanel { Spacing = 4, Children = { box, legend } };
    }

    private static void Redraw(ChartLineViewModel line, Polyline polyline, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ChartLineViewModel.Points))
        {
            polyline.Points = ParsePoints(line.Points);
        }
    }

    private static Control Vector(LiveDisplayViewModel model)
    {
        var canvas = new Canvas { Width = 100, Height = 100 };
        canvas.Children.Add(new Rectangle { Width = 100, Height = 100, Stroke = Brushes.Gray, StrokeThickness = 0.5 });
        canvas.Children.Add(new Line { StartPoint = new Point(50, 0), EndPoint = new Point(50, 100), Stroke = Brushes.LightGray, StrokeThickness = 0.5 });
        canvas.Children.Add(new Line { StartPoint = new Point(0, 50), EndPoint = new Point(100, 50), Stroke = Brushes.LightGray, StrokeThickness = 0.5 });
        var trail = new Polyline { StrokeThickness = 1, Opacity = 0.5, Stroke = Brush.Parse(model.PlotColor), Points = ParsePoints(model.PlotTrail) };
        var point = new Ellipse { Width = 5, Height = 5, Fill = Brush.Parse(model.PlotColor) };
        canvas.Children.Add(trail);
        canvas.Children.Add(point);
        var readout = new TextBlock { FontFamily = new FontFamily("Consolas,DejaVu Sans Mono,monospace"), Text = model.Readout };

        void Place()
        {
            Canvas.SetLeft(point, model.PlotX - 2.5);
            Canvas.SetTop(point, model.PlotY - 2.5);
            trail.Points = ParsePoints(model.PlotTrail);
            trail.Stroke = Brush.Parse(model.PlotColor);
            point.Fill = Brush.Parse(model.PlotColor);
            readout.Text = model.Readout;
        }

        model.PropertyChanged += (_, _) => Place();
        Place();
        return new StackPanel { Children = { new Viewbox { Width = 160, Height = 160, HorizontalAlignment = HorizontalAlignment.Left, Child = canvas }, readout } };
    }
}
