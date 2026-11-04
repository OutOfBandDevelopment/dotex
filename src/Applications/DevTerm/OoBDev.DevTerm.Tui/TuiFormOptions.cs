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

/// <summary>What a host adds to a <see cref="FormRenderer"/> form: its own widget for a control, or a button's action.</summary>
internal sealed class TuiFormOptions
{
    /// <summary>Replaces a control's widget (the form keeps its label, column and visibility); keyed by <see cref="UiControl.Id"/>.</summary>
    public Dictionary<string, Func<UiControl, TuiCustomWidget>> CustomWidgets { get; } = new(StringComparer.Ordinal);

    /// <summary>What a <see cref="ButtonControl"/> does, keyed by its id; without one, the bound model's command property of that name runs.</summary>
    public Dictionary<string, Action> Actions { get; } = new(StringComparer.Ordinal);

    /// <summary>A text field's picker, keyed by control id: given the field's current text, returns the new text or null when cancelled. The field gets a <c>Pick...</c> button.</summary>
    public Dictionary<string, Func<string, string?>> TextPickers { get; } = new(StringComparer.Ordinal);

    /// <summary>The columns the form may use (for wrapping choices and indicator text); the screen width less the window's frame by default.</summary>
    public int? AvailableWidth { get; set; }

    /// <summary>Section headers become toggles that collapse their rows ("[-] Serial"), remembered per definition; off, headers are plain "── Serial ──" labels.</summary>
    public bool CollapsibleSections { get; set; }
}
