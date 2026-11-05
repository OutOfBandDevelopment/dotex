using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using OoBDev.DevTerm.Configuration;
using OoBDev.UiDefinitions;
using OoBDev.UiDefinitions.Forms;
using Terminal.Gui.App;
using Terminal.Gui.Input;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
namespace OoBDev.DevTerm.Tui;

/// <summary>One control's views (each at a line offset within the row), its height, and how to re-read it from the model.</summary>
internal sealed class FormRow(UiControl control)
{
    private readonly Dictionary<View, int> _columnOffsets = [];

    public UiControl Control { get; } = control;

    public List<(View View, int Line, Func<bool>? ShowWhen)> Items { get; } = [];

    /// <summary>The row's "Label:" (null for a toggle, a button, or an empty label) — it stays at the indent; everything else follows the section's widget column.</summary>
    public Label? Label { get; set; }

    /// <summary>Remembers where each widget sits relative to <paramref name="column"/>, the column it was built at, so <see cref="MoveToColumn"/> can move it.</summary>
    public void CaptureColumnOffsets(int column)
    {
        foreach (var (view, _, _) in Items)
        {
            if (view != Label && view.X is PosAbsolute absolute)
            {
                _columnOffsets[view] = absolute.Position - column;
            }
        }
    }

    /// <summary>Puts the row's widgets at <paramref name="column"/> (views positioned relative to another view follow it).</summary>
    public void MoveToColumn(int column)
    {
        foreach (var (view, offset) in _columnOffsets)
        {
            view.X = column + offset;
        }
    }

    public int Height { get; set; } = 1;

    public int Top { get; set; }

    public Action? Refresh { get; set; }

    public bool FocusWired { get; set; }

    public void Add(View view, int line = 0, Func<bool>? showWhen = null) => Items.Add((view, line, showWhen));

    public void SetVisible(bool visible)
    {
        foreach (var (view, _, showWhen) in Items)
        {
            view.Visible = visible && (showWhen?.Invoke() ?? true);
        }
    }

    public void Place(int top)
    {
        foreach (var (view, line, _) in Items)
        {
            view.Y = top + line;
        }
    }
}
