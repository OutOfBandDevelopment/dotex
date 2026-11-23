using System.Collections.Generic;
namespace OoBDev.DevTerm.Web.Docking;

/// <summary>Children side by side (<c>row</c>) or stacked (<c>column</c>).</summary>
/// <param name="Direction"><c>row</c> or <c>column</c>.</param>
/// <param name="Ratios">Child sizes; normalised to sum to 1.</param>
/// <param name="Children">The child nodes.</param>
public sealed record DockSplit(string Direction, IReadOnlyList<double> Ratios, IReadOnlyList<DockNode> Children) : DockNode;
