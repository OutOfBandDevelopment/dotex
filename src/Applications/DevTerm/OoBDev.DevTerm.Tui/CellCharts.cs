using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using OoBDev.DevTerm.Configuration;
using OoBDev.UiDefinitions;
namespace OoBDev.DevTerm.Tui;

/// <summary>
/// Character-cell renderings of the live display controls for the TUI: block-character bars, a
/// braille strip chart, and braille vector/coordinate plots. Pure functions of the shared
/// <see cref="LiveDisplayState"/>, so they're tested directly, without a terminal.
/// </summary>
internal static class CellCharts
{
    /// <summary>Muted ink for axes, rings and trails: the theme's <c>chartMuted</c>.</summary>
    internal static string Muted => ActiveTheme.Current[ThemeRole.ChartMuted].ToHex();

    /// <summary>Whether the current theme uses <see cref="ChartPalette.DarkSlots"/> (same hues and order, stepped for a dark surface).</summary>
    private static bool DarkPalette => ActiveTheme.Current.ChartPalette == ChartPaletteVariant.Dark;

    internal const int BarWidth = 30;
    internal const int StripPlotWidth = 40;
    internal const int StripPlotHeight = 8;
    internal const int VectorWidth = 21;
    internal const int VectorHeight = 9;

    private const string _eighths = " ▏▎▍▌▋▊▉█";

    /// <summary>One row per channel: <c>A ▕██████▌        ▏ 42.1 %</c> — the bar in the channel's color, the value in the normal ink.</summary>
    public static CellGrid RenderBarGraph(BarGraphState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        var channels = state.Control.Channels;
        var labelWidth = channels.Count == 0 ? 0 : channels.Max(c => (c.Label ?? c.Id).Length);
        var grid = new CellGrid(labelWidth + BarWidth + 16, Math.Max(channels.Count, 1));
        for (var row = 0; row < channels.Count; row++)
        {
            var channel = channels[row];
            var x = grid.Write(0, row, (channel.Label ?? channel.Id).PadRight(labelWidth));
            x = grid.Write(x, row, " ▕", Muted);
            var color = ChartPalette.ColorFor(channel, row, DarkPalette);
            var eighths = (int)Math.Round(state.FractionOf(channel.Id) * BarWidth * 8);
            for (var i = 0; i < BarWidth; i++)
            {
                var filled = Math.Clamp(eighths - (i * 8), 0, 8);
                grid.Set(x + i, row, _eighths[filled], color);
            }

            x = grid.Write(x + BarWidth, row, "▏ ", Muted);
            grid.Write(x, row, state.ValueOf(channel.Id) is { } value ? ChartValue.Format(value, state.Control.Unit) : "—");
        }

        return grid;
    }

    /// <summary>
    /// A braille line plot of every channel's history (newest at the right edge), a value axis with
    /// its top/bottom labels on the left, and a legend row naming each channel with its latest value.
    /// </summary>
    public static CellGrid RenderStripChart(StripChartState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        var (minimum, maximum) = state.Scale();
        var top = ChartValue.Format(maximum);
        var bottom = ChartValue.Format(minimum);
        var axisWidth = Math.Max(top.Length, bottom.Length) + 2;
        var grid = new CellGrid(axisWidth + StripPlotWidth, StripPlotHeight + 1);

        for (var row = 0; row < StripPlotHeight; row++)
        {
            var label = row == 0 ? top : row == StripPlotHeight - 1 ? bottom : string.Empty;
            grid.Write(0, row, label.PadLeft(axisWidth - 2));
            grid.Set(axisWidth - 1, row, row == 0 || row == StripPlotHeight - 1 ? '┤' : '│', Muted);
        }

        var canvas = new BrailleCanvas(StripPlotWidth, StripPlotHeight);
        var capacity = state.Capacity;
        var span = maximum - minimum;
        for (var index = 0; index < state.Control.Channels.Count; index++)
        {
            var channel = state.Control.Channels[index];
            var color = ChartPalette.ColorFor(channel, index, DarkPalette);
            var samples = state.SamplesOf(channel.Id);
            (int X, int Y)? previous = null;
            for (var i = 0; i < samples.Count; i++)
            {
                var slot = capacity - samples.Count + i;
                var x = capacity <= 1 ? canvas.DotWidth - 1 : (int)Math.Round(slot * (canvas.DotWidth - 1) / (double)(capacity - 1));
                var fraction = span <= 0 ? 0.5 : Math.Clamp((samples[i] - minimum) / span, 0, 1);
                var y = (int)Math.Round((1 - fraction) * (canvas.DotHeight - 1));
                if (previous is { } p)
                {
                    canvas.Line(p.X, p.Y, x, y, color);
                }
                else
                {
                    canvas.Plot(x, y, color);
                }

                previous = (x, y);
            }
        }

        canvas.DrawInto(grid, axisWidth, 0);

        var legendX = axisWidth;
        for (var index = 0; index < state.Control.Channels.Count; index++)
        {
            var channel = state.Control.Channels[index];
            var samples = state.SamplesOf(channel.Id);
            legendX = grid.Write(legendX, StripPlotHeight, "■", ChartPalette.ColorFor(channel, index, DarkPalette));
            var latest = samples.Count > 0 ? ChartValue.Format(samples[^1], state.Control.Unit) : "—";
            legendX = grid.Write(legendX, StripPlotHeight, $" {channel.Label ?? channel.Id} {latest}   ");
        }

        return grid;
    }

    /// <summary>
    /// A braille plot of the current point (a 2×2 dot block in the live h/s/v color, or the first
    /// palette color) and its fading trail, on crosshair axes — plus a ring at <c>Range</c> for polar
    /// and an oblique z axis for x/y/z — with a readout row below (<c>x=0.5 y=-0.25</c>).
    /// </summary>
    public static CellGrid RenderVector(VectorState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        var grid = new CellGrid(VectorWidth + 9, VectorHeight + 1);
        var canvas = new BrailleCanvas(VectorWidth, VectorHeight);
        var centerX = canvas.DotWidth / 2;
        var centerY = canvas.DotHeight / 2;
        var radius = (Math.Min(canvas.DotWidth, canvas.DotHeight) / 2) - 1;
        var control = state.Control;

        // Crosshair axes as box-drawing characters, underneath any dots.
        var midColumn = centerX / 2;
        var midRow = centerY / 4;
        for (var x = 0; x < VectorWidth; x++)
        {
            grid.Set(x, midRow, '─', Muted);
        }

        for (var y = 0; y < VectorHeight; y++)
        {
            grid.Set(midColumn, y, y == midRow ? '┼' : '│', Muted);
        }

        // The point spans ±Range; x/y/z shrinks the x/y extent so the oblique z offset still fits.
        var isXyz = control.Coordinates == CoordinateSystem.XYZ;
        var range = control.Range > 0 ? control.Range : 1;
        var scale = radius / range * (isXyz ? 0.7 : 1);

        (int X, int Y) Project((double X, double Y, double Z) p)
        {
            var zx = isXyz ? p.Z * 0.35 * Math.Cos(Math.PI / 4) : 0;
            var zy = isXyz ? p.Z * 0.35 * Math.Sin(Math.PI / 4) : 0;
            return ((int)Math.Round(centerX + ((p.X + zx) * scale)), (int)Math.Round(centerY - ((p.Y + zy) * scale)));
        }

        if (control.Coordinates == CoordinateSystem.Polar)
        {
            for (var degrees = 0; degrees < 360; degrees += 4)
            {
                var radians = degrees * Math.PI / 180;
                canvas.Plot((int)Math.Round(centerX + (radius * Math.Cos(radians))), (int)Math.Round(centerY - (radius * Math.Sin(radians))), Muted);
            }
        }

        if (isXyz)
        {
            var end = Project((0, 0, range));
            var start = Project((0, 0, -range));
            canvas.Line(start.X, start.Y, end.X, end.Y, Muted);
        }

        foreach (var point in state.Trail)
        {
            var (x, y) = Project(point);
            canvas.Plot(x, y, Muted);
        }

        if (state.HasPoint)
        {
            var color = state.Color is { } c ? ToHex(c) : ChartPalette.SlotsFor(DarkPalette)[0];
            var (x, y) = Project(state.Point);
            if (control.Coordinates == CoordinateSystem.Polar)
            {
                canvas.Line(centerX, centerY, x, y, color);
            }

            canvas.Plot(x, y, color);
            canvas.Plot(x + 1, y, color);
            canvas.Plot(x, y + 1, color);
            canvas.Plot(x + 1, y + 1, color);
        }

        canvas.DrawInto(grid, 0, 0);
        grid.Write(0, VectorHeight, state.Readout());
        return grid;
    }

    private static string ToHex((byte R, byte G, byte B) color) => $"#{color.R:X2}{color.G:X2}{color.B:X2}";
}
