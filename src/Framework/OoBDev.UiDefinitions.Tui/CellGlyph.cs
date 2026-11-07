using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using OoBDev.UiDefinitions;
namespace OoBDev.UiDefinitions.Tui;

/// <summary>One character cell of a chart: its glyph and an optional <c>#RRGGBB</c> foreground (null draws in the view's normal color).</summary>
public readonly record struct CellGlyph(char Char, string? Color);

/// <summary>A fixed-size grid of <see cref="CellGlyph"/>s — what a TUI chart renders to, before <see cref="CellCanvasView"/> draws it.</summary>
public sealed class CellGrid
{
    private readonly CellGlyph[,] _cells;

    public CellGrid(int width, int height)
    {
        Width = Math.Max(width, 1);
        Height = Math.Max(height, 1);
        _cells = new CellGlyph[Width, Height];
        for (var y = 0; y < Height; y++)
        {
            for (var x = 0; x < Width; x++)
            {
                _cells[x, y] = new CellGlyph(' ', null);
            }
        }
    }

    public int Width { get; }

    public int Height { get; }

    public CellGlyph this[int x, int y] => _cells[x, y];

    public void Set(int x, int y, char c, string? color)
    {
        if (x >= 0 && x < Width && y >= 0 && y < Height)
        {
            _cells[x, y] = new CellGlyph(c, color);
        }
    }

    /// <summary>Writes <paramref name="text"/> left to right from (x, y), clipped at the right edge; returns the column after it.</summary>
    public int Write(int x, int y, string text, string? color = null)
    {
        foreach (var c in text)
        {
            Set(x++, y, c, color);
        }

        return x;
    }

    /// <summary>The grid's text, one string per row (trailing spaces kept) — what tests assert on.</summary>
    public IReadOnlyList<string> Lines()
    {
        var lines = new List<string>(Height);
        for (var y = 0; y < Height; y++)
        {
            var builder = new StringBuilder(Width);
            for (var x = 0; x < Width; x++)
            {
                builder.Append(_cells[x, y].Char);
            }

            lines.Add(builder.ToString());
        }

        return lines;
    }
}
