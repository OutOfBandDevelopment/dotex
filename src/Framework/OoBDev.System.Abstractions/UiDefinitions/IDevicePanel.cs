namespace OoBDev.UiDefinitions;

/// <summary>
/// A device's control panel as data: the <see cref="UiDefinition"/>, the surface that executes its commands and the
/// decoder that feeds its indicators. A device library implements it beside its protocol; a front end adapts it to its menus.
/// </summary>
public interface IDevicePanel
{
    /// <summary>A stable id (also what <c>Web:Panel</c> selects), for example <c>k8055</c>.</summary>
    string Id { get; }

    /// <summary>The menu text, with an <c>_</c> before the access key.</summary>
    string MenuTitle { get; }

    /// <summary>The name of the decoder that feeds the panel's indicators.</summary>
    string PresenterName { get; }

    /// <summary>Whether the panel suits a connection.</summary>
    /// <param name="transport">The transport name, for example <c>hid</c>.</param>
    /// <param name="vendorId">The USB vendor id, or 0.</param>
    /// <param name="productId">The USB product id, or 0.</param>
    bool IsAvailable(string transport, int vendorId, int productId);

    /// <summary>Builds the panel layout.</summary>
    UiDefinition BuildDefinition();

    /// <summary>Creates the surface that runs the panel's commands over <paramref name="send"/>.</summary>
    /// <param name="send">Sends bytes to the device.</param>
    /// <param name="decoder">The decoder of the same session (what <see cref="CreateDecoder"/> returned for it), so a surface can read the replies the session already decodes instead of reading the connection a second time.</param>
    IControlSurface CreateSurface(DeviceSend send, IDeviceDecoder decoder);

    /// <summary>Creates a fresh decoder for one session.</summary>
    IDeviceDecoder CreateDecoder();
}
