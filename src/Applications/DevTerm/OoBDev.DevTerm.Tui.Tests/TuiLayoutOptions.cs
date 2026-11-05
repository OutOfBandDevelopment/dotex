using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Drawing;
using System.Text;
using Terminal.Gui.App;
using Terminal.Gui.ViewBase;
using Terminal.Gui.Views;
namespace OoBDev.DevTerm.Tui.Tests;

/// <summary>Deliberate exceptions for <see cref="TuiLayoutAssert"/> - each should say why in a comment at the call site.</summary>
internal sealed class TuiLayoutOptions
{
    /// <summary>Views allowed to overlap their siblings (an overlay, a popover).</summary>
    public List<View> AllowOverlap { get; } = [];

    /// <summary>Views (and everything under them) left out of every check.</summary>
    public List<View> Ignore { get; } = [];

    /// <summary>Also compare what's in the screen buffer with each fully visible text view's text. Off when something is drawn over the window (an open menu, a dialog).</summary>
    public bool CheckRenderedText { get; set; } = true;

    internal bool AllowsOverlap(View view) => AllowOverlap.Contains(view);

    internal bool Ignores(View view) => Ignore.Contains(view);
}
