using System;
namespace OoBDev.UiDefinitions.Tui;

/// <summary>
/// A braille-dot canvas: each character cell holds a 2×4 grid of dots (U+2800–U+28FF), so a
/// terminal plot gets 2× horizontal and 4× vertical resolution — with a cell about twice as tall
/// as it is wide, the dots come out roughly square. A cell's color is the last one plotted into it.
/// </summary>
public sealed class BrailleCanvas
{
    // Dot bit for (column, row) within a cell — the Unicode braille pattern numbering.
    private static readonly int[,] _dotBits = { { 0x01, 0x02, 0x04, 0x40 }, { 0x08, 0x10, 0x20, 0x80 } };

    private readonly int[,] _bits;
    private readonly string?[,] _colors;

    public BrailleCanvas(int widthCells, int heightCells)
    {
        WidthCells = Math.Max(widthCells, 1);
        HeightCells = Math.Max(heightCells, 1);
        _bits = new int[WidthCells, HeightCells];
        _colors = new string?[WidthCells, HeightCells];
    }

    public int WidthCells { get; }

    public int HeightCells { get; }

    public int DotWidth => WidthCells * 2;

    public int DotHeight => HeightCells * 4;

    public void Plot(int x, int y, string? color)
    {
        if (x < 0 || y < 0 || x >= DotWidth || y >= DotHeight)
        {
            return;
        }

        _bits[x / 2, y / 4] |= _dotBits[x % 2, y % 4];
        _colors[x / 2, y / 4] = color;
    }

    /// <summary>A straight line of dots between two points (Bresenham).</summary>
    public void Line(int x0, int y0, int x1, int y1, string? color)
    {
        var dx = Math.Abs(x1 - x0);
        var dy = -Math.Abs(y1 - y0);
        var sx = x0 < x1 ? 1 : -1;
        var sy = y0 < y1 ? 1 : -1;
        var error = dx + dy;
        while (true)
        {
            Plot(x0, y0, color);
            if (x0 == x1 && y0 == y1)
            {
                return;
            }

            var e2 = 2 * error;
            if (e2 >= dy)
            {
                error += dy;
                x0 += sx;
            }

            if (e2 <= dx)
            {
                error += dx;
                y0 += sy;
            }
        }
    }

    /// <summary>Copies every cell that has at least one dot into <paramref name="grid"/> at (<paramref name="left"/>, <paramref name="top"/>); empty cells leave what's already there (axes, labels).</summary>
    public void DrawInto(CellGrid grid, int left, int top)
    {
        for (var y = 0; y < HeightCells; y++)
        {
            for (var x = 0; x < WidthCells; x++)
            {
                if (_bits[x, y] != 0)
                {
                    grid.Set(left + x, top + y, (char)(0x2800 + _bits[x, y]), _colors[x, y]);
                }
            }
        }
    }
}
