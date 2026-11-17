using OoBDev.UiDefinitions;

namespace OoBDev.DevTerm;

/// <summary>
/// A device control panel a plugin contributes (docs/design/Plugins/plugin-contributed-panels.md): register one from an
/// <see cref="Plugins.IPluginModule"/> and every front end adds a Device-menu entry for it, enabled when the connection
/// suits the device. The panel itself is the generic <see cref="UiDefinition"/> renderer, so a contribution is data plus a surface.
/// </summary>
public interface IDevicePanelContribution
{
    /// <summary>A stable id (also what <c>Web:Panel</c> selects), e.g. "k8055".</summary>
    string Id { get; }

    /// <summary>The Device menu text, with an <c>_</c> before the access key, e.g. "_K8055 Control Panel...".</summary>
    string MenuTitle { get; }

    /// <summary>The structured presenter that feeds the panel's indicators, or null when it has none.</summary>
    string? PresenterName { get; }

    /// <summary>Whether the panel makes sense for a connection (the menu item is disabled otherwise). Called only while connected.</summary>
    bool IsAvailable(string transport, int vendorId, int productId);

    UiDefinition BuildDefinition();

    /// <summary>Creates the surface for the session.</summary>
    /// <param name="session">The session the surface sends through.</param>
    /// <param name="source">The session's presenter that feeds the panel (the catalog entry named <see cref="PresenterName"/>), or null; a surface that reads replies uses its decoder.</param>
    IControlSurface CreateSurface(Session session, IStructuredPresenter? source = null);
}
