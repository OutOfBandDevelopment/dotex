using System;
using OoBDev.UiDefinitions;

namespace OoBDev.Zoom.H4n.Panels;

/// <summary>The Zoom H4n remote panel as an <see cref="IDevicePanel"/>.</summary>
/// <remarks>The remote port is a plain serial line (2400 baud) with no USB identity, so the panel is offered on every serial connection.</remarks>
public sealed class ZoomH4nPanel : IDevicePanel
{
    /// <inheritdoc />
    public string Id => ZoomGlobals.ProviderKey;

    /// <inheritdoc />
    public string MenuTitle => "_Zoom H4n Remote...";

    /// <inheritdoc />
    public string PresenterName => "zoom-h4n";

    /// <inheritdoc />
    public bool IsAvailable(string transport, int vendorId, int productId) =>
        string.Equals(transport, "serial", StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public UiDefinition BuildDefinition() => ZoomH4nUiDefinition.Build();

    /// <inheritdoc />
    public IControlSurface CreateSurface(DeviceSend send, IDeviceDecoder decoder) => new ZoomH4nControlSurface(send);

    /// <inheritdoc />
    public IDeviceDecoder CreateDecoder() => new ZoomH4nDecoder();
}
