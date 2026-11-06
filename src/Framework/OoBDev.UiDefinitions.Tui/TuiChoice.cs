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
using OoBDev.UiDefinitions;
using OoBDev.UiDefinitions.Forms;
using Terminal.Gui.App;
using Terminal.Gui.Input;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
namespace OoBDev.UiDefinitions.Tui;

/// <summary>A single-choice control, whichever widget it rendered as — read or set its value by option text (setting it drives the real widget, whose own change event updates the model).</summary>
public sealed class TuiChoice(IReadOnlyList<string> options, View widget, Func<string?> get, Action<string?> set)
{
    public IReadOnlyList<string> Options { get; } = options;

    /// <summary>The <see cref="OptionSelector"/>, the first radio check box, or the pick text field.</summary>
    public View Widget { get; } = widget;

    public string? Value
    {
        get => get();
        set => set(value);
    }
}
