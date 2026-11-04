using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Drawing;
using System.Globalization;
using System.Text;
using OoBDev.DevTerm.Configuration;
using OoBDev.DevTerm;
using OoBDev.DevTerm.Presenters.Text;
using OoBDev.UiDefinitions;
using Terminal.Gui.App;
using Terminal.Gui.Editor;
using Terminal.Gui.Input;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
namespace OoBDev.DevTerm.Tui;

/// <summary>The controls a test needs to drive a rendered control panel headlessly.</summary>
internal sealed class ControlPanelWindowParts
{
    public required Window Window { get; init; }

    /// <summary>Every interactive/display view, keyed by its <c>UiControl.Id</c> — a dictionary rather than named properties (as <see cref="ConfigureWindowParts"/> uses) because this renderer's control set is data-driven, not fixed at compile time.</summary>
    public required IReadOnlyDictionary<string, View> ControlViews { get; init; }

    /// <summary>The subset of <see cref="ControlViews"/> that are <see cref="IndicatorControl"/> labels, for tests asserting a live value update.</summary>
    public required IReadOnlyDictionary<string, Label> IndicatorLabels { get; init; }

    /// <summary>The bar graph / strip chart / vector displays, keyed by <c>UiControl.Id</c> — each with its live state and its rendered cells.</summary>
    public required IReadOnlyDictionary<string, CellCanvasView> DisplayViews { get; init; }

    /// <summary>The scrolling form holding every section — its <c>Viewport</c> is the visible part of the content (scrolled vertically and horizontally).</summary>
    public required View FormContent { get; init; }

    /// <summary>The <c>(i)</c> marker next to each control that sends a previewable command, keyed by <c>UiControl.Id</c>.</summary>
    public required IReadOnlyDictionary<string, Label> InfoMarkers { get; init; }

    /// <summary>Each labeled section's <c>[-]</c>/<c>[+]</c> expand/collapse header, keyed by section label (<see cref="ControlPanelMode.NotesSectionLabel"/> for the notes).</summary>
    public required IReadOnlyDictionary<string, Button> SectionHeaders { get; init; }

    /// <summary>Each section's container of rows, keyed by section label (an unlabeled section under the empty string).</summary>
    public required IReadOnlyDictionary<string, View> SectionBodies { get; init; }

    /// <summary>The footer line showing what the focused control would send (<c>Sends: ...</c>).</summary>
    public required Label PreviewLabel { get; init; }

    /// <summary>The footer line reporting a rejected (invalid, not sent) value.</summary>
    public required Label MessageLabel { get; init; }
}
