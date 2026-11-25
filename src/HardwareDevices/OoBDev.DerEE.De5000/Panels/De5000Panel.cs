using System;
using OoBDev.UiDefinitions;

namespace OoBDev.DerEE.De5000.Panels;

/// <summary>The DE-5000 live readings as an <see cref="IDevicePanel"/>. The meter is reached through an optical-to-USB serial adapter or an optical-to-BLE adapter, neither with a USB identity of the meter, so the panel is offered on serial and BLE connections.</summary>
public sealed class De5000Panel : IDevicePanel
{
    /// <inheritdoc />
    public string Id => DerEEGlobals.ProviderKey;

    /// <inheritdoc />
    public string MenuTitle => "_DE-5000 LCR Meter...";

    /// <inheritdoc />
    public string PresenterName => DerEEGlobals.ProviderKey;

    /// <inheritdoc />
    public bool IsAvailable(string transport, int vendorId, int productId) =>
        string.Equals(transport, "serial", StringComparison.OrdinalIgnoreCase) || string.Equals(transport, "ble", StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public UiDefinition BuildDefinition() => De5000UiDefinition.Build();

    /// <inheritdoc />
    public IControlSurface CreateSurface(DeviceSend send, IDeviceDecoder decoder) => new De5000ControlSurface();

    /// <inheritdoc />
    public IDeviceDecoder CreateDecoder() => new De5000Decoder();
}
