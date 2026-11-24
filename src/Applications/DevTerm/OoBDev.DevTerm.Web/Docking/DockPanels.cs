using System.Collections.Generic;
using OoBDev.DevTerm.Web.Components;
namespace OoBDev.DevTerm.Web.Docking;

/// <summary>The registered panels: a layout can only name these, so a saved or imported layout cannot reach any other component.</summary>
public static class DockPanels
{
    /// <summary>Every dockable page in nav order.</summary>
    public static IReadOnlyList<DockPanel> All { get; } =
    [
        new("profiles", "Profiles", typeof(Profiles)),
        new("connections", "Connections", typeof(Connections)),
        new("panel", "Panel", typeof(Panel)),
        new("playback", "Playback", typeof(Playback)),
        new("monitor", "Monitor", typeof(Monitor)),
        new("routing", "Routing", typeof(Routing)),
        new("themes", "Themes", typeof(Themes)),
        new("converters", "Converters", typeof(Converters)),
        new("manifest", "Manifest", typeof(Manifest)),
        new("plugins", "Plugins", typeof(Plugins)),
    ];
}
