using System;
namespace OoBDev.DevTerm.Web.Docking;

/// <summary>The actions the rendered dock tree raises; the host turns them into layout operations.</summary>
/// <param name="Activate">A tab was chosen.</param>
/// <param name="Close">A tab was closed.</param>
/// <param name="Drag">A tab drag started.</param>
/// <param name="Drop">The dragged tab was dropped on a pane: direction null moves it into the pane, "row" or "column" splits the pane (before = left or top).</param>
/// <param name="Resize">The ratio between a split child and its next sibling moved by the given amount.</param>
public sealed record DockEvents(
    Action<string> Activate,
    Action<string> Close,
    Action<string> Drag,
    Action<DockPane, string?, bool> Drop,
    Action<DockSplit, int, double> Resize);
