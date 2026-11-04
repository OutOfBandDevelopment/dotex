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

/// <summary>One rendered form: its root view plus the lookups a host or test needs, keyed by <see cref="UiControl.Id"/>.</summary>
internal sealed class TuiFormParts
{
    private int _refreshing;

    internal TuiFormParts(FormBinding binding)
    {
        Binding = binding;
    }

    public required View Root { get; init; }

    public FormBinding Binding { get; }

    /// <summary>Each control's main widget (text field, check box, option selector, label, button, or the host's own).</summary>
    public Dictionary<string, View> ControlViews { get; } = new(StringComparer.Ordinal);

    /// <summary>Single-choice controls by id, whatever widget each became.</summary>
    public Dictionary<string, TuiChoice> Choices { get; } = new(StringComparer.Ordinal);

    /// <summary>A check list's check boxes, or a wrapped single choice's radio check boxes, in option order.</summary>
    public Dictionary<string, IReadOnlyList<CheckBox>> CheckLists { get; } = new(StringComparer.Ordinal);

    /// <summary>The <c>Pick...</c> button beside a long choice's text field.</summary>
    public Dictionary<string, Button> PickButtons { get; } = new(StringComparer.Ordinal);

    public Dictionary<string, Label> RowLabels { get; } = new(StringComparer.Ordinal);

    /// <summary>The inline <c>! message</c> after a text field whose value fails its constraint.</summary>
    public Dictionary<string, Label> ErrorLabels { get; } = new(StringComparer.Ordinal);

    public Dictionary<string, View> SectionHeaderLabels { get; } = new(StringComparer.Ordinal);

    /// <summary>The form's height as last laid out.</summary>
    public int Rows { get; private set; }

    /// <summary>A widget in the form took focus: its row's content-relative (top, height) — for a scrolling host to reveal it.</summary>
    public event Action<int, int>? RowFocused;

    /// <summary>The form's layout changed (a section or row was shown or hidden, a wrapped text changed height).</summary>
    public event EventHandler? Reflowed;

    internal List<SectionRows> Sections { get; } = [];

    internal bool IsRefreshing => _refreshing > 0;

    /// <summary>Whether <paramref name="id"/>'s row is currently shown (its section's and its own condition both hold).</summary>
    public bool IsRowVisible(string id) =>
        Sections.Any(s => Binding.IsVisible(s.Section.VisibleWhen) && s.Rows.Any(r => r.Control.Id == id && Binding.IsVisible(r.Control.VisibleWhen)));

    /// <summary>Re-reads every widget from the model and re-lays out the form.</summary>
    public void RefreshAll()
    {
        foreach (var row in Sections.SelectMany(s => s.Rows))
        {
            RefreshRow(row);
        }

        Reflow();
    }

    internal void OnModelChanged(string? name)
    {
        foreach (var row in Sections.SelectMany(s => s.Rows))
        {
            if (name is null || row.Control.Id == name || row.Control is IndicatorControl)
            {
                RefreshRow(row);
            }
        }

        Reflow();
    }

    /// <summary>Runs a widget → model write, unless the widget is only changing because the model is being read into it.</summary>
    internal void Push(Action write)
    {
        if (_refreshing == 0)
        {
            write();
        }
    }

    internal void RefreshRow(FormRow row)
    {
        if (row.Refresh is null)
        {
            return;
        }

        _refreshing++;
        try
        {
            row.Refresh();
        }
        finally
        {
            _refreshing--;
        }
    }

    /// <summary>Re-lays out after a section header was toggled.</summary>
    internal void Relayout() => Reflow();

    private void Reflow()
    {
        var y = 0;
        var any = false;
        foreach (var section in Sections)
        {
            var sectionVisible = Binding.IsVisible(section.Section.VisibleWhen);
            var shownRows = section.Rows.Where(r => sectionVisible && Binding.IsVisible(r.Control.VisibleWhen)).ToList();
            var shown = sectionVisible && shownRows.Count > 0;
            if (section.Header is { } header)
            {
                header.Visible = shown;
            }

            if (!shown)
            {
                foreach (var row in section.Rows)
                {
                    row.SetVisible(false);
                }

                continue;
            }

            if (any)
            {
                y++; // a blank line between sections
            }

            any = true;
            if (section.Header is { } shownHeader)
            {
                shownHeader.Y = y;
                if (shownHeader is Button button)
                {
                    button.Text = section.HeaderText;
                }

                y++;
            }

            if (!section.Expanded)
            {
                foreach (var row in section.Rows)
                {
                    row.SetVisible(false);
                }

                continue;
            }

            // The widget column follows the longest label among the rows actually shown, so a hidden
            // row's long label (e.g. "Detected USBTMC devices" while HID is selected) doesn't push the rest right.
            var labelWidth = shownRows.Where(r => r.Label is not null).Select(r => r.Control.Label.Length + 1).DefaultIfEmpty(0).Max();
            var column = FormRenderer.Indent + (labelWidth > 0 ? labelWidth + 1 : 0);
            foreach (var row in section.Rows)
            {
                var visible = shownRows.Contains(row);
                row.SetVisible(visible);
                if (visible)
                {
                    row.Top = y;
                    row.Place(y);
                    row.MoveToColumn(column);
                    y += row.Height;
                }
            }
        }

        var height = Math.Max(y, 1);
        var changed = height != Rows;
        Rows = height;
        Root.Height = height;
        if (changed)
        {
            Reflowed?.Invoke(this, EventArgs.Empty);
        }

        foreach (var row in Sections.SelectMany(s => s.Rows).Where(r => !r.FocusWired))
        {
            row.FocusWired = true;
            foreach (var item in row.Items)
            {
                item.View.HasFocusChanged += (_, e) =>
                {
                    if (e.NewValue)
                    {
                        RowFocused?.Invoke(row.Top, row.Height);
                    }
                };
            }
        }
    }
}
