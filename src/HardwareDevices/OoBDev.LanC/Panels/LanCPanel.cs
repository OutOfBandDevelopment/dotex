using System;
using OoBDev.UiDefinitions;

namespace OoBDev.LanC.Panels;

/// <summary>The LANC remote as an <see cref="IDevicePanel"/>. The bridge is a plain serial line with no USB identity, so it is offered on every serial connection.</summary>
public sealed class LanCPanel : IDevicePanel
{
    /// <inheritdoc />
    public string Id => "lanc";

    /// <inheritdoc />
    public string MenuTitle => "_LANC Remote...";

    /// <inheritdoc />
    public string PresenterName => "lanc";

    /// <inheritdoc />
    public bool IsAvailable(string transport, int vendorId, int productId) =>
        string.Equals(transport, "serial", StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public UiDefinition BuildDefinition() => LanCUiDefinition.Build();

    /// <inheritdoc />
    public IControlSurface CreateSurface(DeviceSend send, IDeviceDecoder decoder) => new LanCControlSurface(send);

    /// <inheritdoc />
    public IDeviceDecoder CreateDecoder() => new LanCDecoder();
}
