using System;
namespace OoBDev.DevTerm.Web.Docking;

/// <summary>A page that can be placed in the dock.</summary>
/// <param name="Id">Stable id saved in the layout.</param>
/// <param name="Title">Tab text.</param>
/// <param name="Component">The Blazor component that renders it.</param>
public sealed record DockPanel(string Id, string Title, Type Component);
