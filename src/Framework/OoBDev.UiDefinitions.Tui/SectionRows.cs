using System.Collections.Generic;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
namespace OoBDev.UiDefinitions.Tui;

/// <summary>One section's header (null when unlabeled) and rows.</summary>
public sealed class SectionRows(UiSection section)
{
    public UiSection Section { get; } = section;

    public View? Header { get; set; }

    /// <summary>Whether the rows show; only ever false for a collapsible form's section the user collapsed.</summary>
    public bool Expanded { get; set; } = true;

    /// <summary>True when the header is a toggle button (<see cref="TuiFormOptions.CollapsibleSections"/>).</summary>
    public bool Collapsible => Header is Button;

    public string HeaderText => Collapsible
        ? $"[{(Expanded ? '-' : '+')}] {Section.Label}"
        : $"── {Section.Label} ──";

    public List<FormRow> Rows { get; } = [];
}
