using System.Collections.Generic;
namespace OoBDev.DevTerm.Web.Docking;

/// <summary>A tab strip.</summary>
/// <param name="Tabs">Panel ids in tab order.</param>
/// <param name="Active">The shown panel id, or null for the first tab.</param>
public sealed record DockPane(IReadOnlyList<string> Tabs, string? Active = null) : DockNode;
