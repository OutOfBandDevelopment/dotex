using System;
using System.Linq;

namespace OoBDev.DevTerm.Configuration;

/// <summary>
/// Whether a Device menu panel makes sense for the current connection - shared by the TUI's and
/// WPF's Device menus, which disable an item that can't work (opening a device panel on a TCP
/// oscilloscope connection, say, or any panel while disconnected) instead of letting it open a
/// panel whose every command would fail.
/// </summary>
public static class DevicePanels
{
    /// <summary>
    /// Why a plugin-contributed panel's menu item is disabled, or null when it is enabled. A panel whose indicators come
    /// from a presenter is disabled (not hidden) until that presenter is among the connection's selected ones.
    /// </summary>
    public static string? UnavailableReason(IDevicePanelContribution panel, CliOptions options, bool connected)
    {
        ArgumentNullException.ThrowIfNull(panel);
        ArgumentNullException.ThrowIfNull(options);

        if (!connected)
        {
            return "Connect first.";
        }

        if (!panel.IsAvailable(options.Transport, options.VendorId, options.ProductId))
        {
            return "This panel does not suit the current connection.";
        }

        if (panel.PresenterName is { Length: > 0 } presenter
            && !options.EffectivePresenters.Contains(presenter, StringComparer.OrdinalIgnoreCase))
        {
            return $"Select the '{presenter}' presenter to use this panel.";
        }

        return null;
    }

    public static bool IsAvailable(DevicePanel panel, CliOptions options, bool connected)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (!connected)
        {
            return false;
        }

        return panel switch
        {
            // A manifest names its own transport and commands, so which manifests make sense is the
            // user's call (the picker) - the item is enabled whenever connected.
            DevicePanel.Manifest => true,
            _ => false,
        };
    }
}
