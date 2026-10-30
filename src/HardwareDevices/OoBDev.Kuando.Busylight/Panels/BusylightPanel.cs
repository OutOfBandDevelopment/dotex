using System;
using OoBDev.UiDefinitions;

namespace OoBDev.Kuando.Busylight.Panels;

/// <summary>The Busylight control panel as an <see cref="IDevicePanel"/>.</summary>
public sealed class BusylightPanel : IDevicePanel
{
    private const int _microchipVendorId = 0x04D8;
    private const int _microchipBusylightProductId = 0xF848;
    private const int _plenomVendorId = 0x27BB;

    /// <inheritdoc />
    public string Id => "busylight";

    /// <inheritdoc />
    public string MenuTitle => "_Busylight Control Panel...";

    /// <inheritdoc />
    public string PresenterName => "busylight";

    /// <inheritdoc />
    /// <remarks>The Microchip-vendor bench unit (0x04D8:0xF848) and any Plenom-vendor device (0x27BB).</remarks>
    public bool IsAvailable(string transport, int vendorId, int productId) =>
        string.Equals(transport, "hid", StringComparison.OrdinalIgnoreCase)
        && ((vendorId == _microchipVendorId && productId == _microchipBusylightProductId) || vendorId == _plenomVendorId);

    /// <inheritdoc />
    public UiDefinition BuildDefinition() => BusylightUiDefinition.Build();

    /// <inheritdoc />
    public IControlSurface CreateSurface(DeviceSend send) => new BusylightControlSurface(send);

    /// <inheritdoc />
    public IDeviceDecoder CreateDecoder() => new BusylightDecoder();
}
