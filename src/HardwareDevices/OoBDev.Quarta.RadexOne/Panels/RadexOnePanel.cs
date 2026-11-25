using System;
using OoBDev.UiDefinitions;

namespace OoBDev.Quarta.RadexOne.Panels;

/// <summary>The Radex One panel as an <see cref="IDevicePanel"/>. The device is a plain serial line (9600 baud) with no USB identity, so it is offered on every serial connection.</summary>
public sealed class RadexOnePanel : IDevicePanel
{
    /// <inheritdoc />
    public string Id => QuartaGlobals.ProviderKey;

    /// <inheritdoc />
    public string MenuTitle => "_Radex One...";

    /// <inheritdoc />
    public string PresenterName => QuartaGlobals.ProviderKey;

    /// <inheritdoc />
    public bool IsAvailable(string transport, int vendorId, int productId) =>
        string.Equals(transport, "serial", StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public UiDefinition BuildDefinition() => RadexOneUiDefinition.Build();

    /// <inheritdoc />
    public IControlSurface CreateSurface(DeviceSend send, IDeviceDecoder decoder) => new RadexOneControlSurface(send);

    /// <inheritdoc />
    public IDeviceDecoder CreateDecoder() => new RadexOneDecoder();
}
