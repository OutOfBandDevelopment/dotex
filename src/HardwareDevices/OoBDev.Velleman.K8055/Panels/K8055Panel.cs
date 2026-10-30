using System;
using OoBDev.UiDefinitions;

namespace OoBDev.Velleman.K8055.Panels;

/// <summary>The K8055 control panel as an <see cref="IDevicePanel"/>.</summary>
public sealed class K8055Panel : IDevicePanel
{
    private const int _vendorId = 0x10CF;
    private const int _firstProductId = 0x5500;
    private const int _lastProductId = 0x5503;

    /// <inheritdoc />
    public string Id => "k8055";

    /// <inheritdoc />
    public string MenuTitle => "_K8055 Control Panel...";

    /// <inheritdoc />
    public string PresenterName => "k8055";

    /// <inheritdoc />
    /// <remarks>One product id per board-address jumper setting (0-3).</remarks>
    public bool IsAvailable(string transport, int vendorId, int productId) =>
        string.Equals(transport, "hid", StringComparison.OrdinalIgnoreCase) && vendorId == _vendorId && productId is >= _firstProductId and <= _lastProductId;

    /// <inheritdoc />
    public UiDefinition BuildDefinition() => K8055UiDefinition.Build();

    /// <inheritdoc />
    public IControlSurface CreateSurface(DeviceSend send) => new K8055ControlSurface(send);

    /// <inheritdoc />
    public IDeviceDecoder CreateDecoder() => new K8055Decoder();
}
