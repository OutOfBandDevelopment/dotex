using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace OoBDev.UiDefinitions.WinForms;

/// <summary>A square-grid plot drawn in a fixed 100 by 100 space scaled to the control, the space the view models publish points in.</summary>
public sealed class PlotSurface : Control
{
    /// <summary>Creates the surface; it paints itself double buffered.</summary>
    public PlotSurface()
    {
        DoubleBuffered = true;
        ResizeRedraw = true;
        BackColor = Color.White;
    }

    /// <summary>One colour and point list per trace, in the 100 by 100 space.</summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public IList<(Color Color, PointF[] Points)> Traces { get; } = new List<(Color Color, PointF[] Points)>();

    /// <summary>A marker (the current vector point), or null.</summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public PointF? Marker { get; set; }

    /// <summary>The marker colour.</summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public Color MarkerColor { get; set; } = Color.SteelBlue;

    /// <summary>True for the vector plot, which has axes through the centre instead of quarter lines.</summary>
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public bool Crosshair { get; set; }

    /// <inheritdoc />
    protected override void OnPaint(PaintEventArgs e)
    {
        base.OnPaint(e);
        var g = e.Graphics;
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.ScaleTransform(Width / 100f, Height / 100f);
        using var grid = new Pen(Color.LightGray, 0.4f);
        using var border = new Pen(Color.Gray, 0.6f);
        if (Crosshair)
        {
            g.DrawLine(grid, 50, 0, 50, 100);
            g.DrawLine(grid, 0, 50, 100, 50);
        }
        else
        {
            foreach (var y in new[] { 25f, 50f, 75f })
            {
                g.DrawLine(grid, 0, y, 100, y);
            }
        }

        g.DrawRectangle(border, 0, 0, 100, 100);
        foreach (var (color, points) in Traces)
        {
            if (points.Length > 1)
            {
                using var pen = new Pen(color, 1.2f);
                g.DrawLines(pen, points);
            }
        }

        if (Marker is { } marker)
        {
            using var brush = new SolidBrush(MarkerColor);
            g.FillEllipse(brush, marker.X - 2.5f, marker.Y - 2.5f, 5, 5);
        }
    }
}
