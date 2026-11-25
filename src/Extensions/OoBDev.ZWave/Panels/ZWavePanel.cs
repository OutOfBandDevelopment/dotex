using System;
using OoBDev.UiDefinitions;

namespace OoBDev.ZWave.Panels;

/// <summary>The DevTerm panel for a Z-Wave USB controller on a serial transport.</summary>
public sealed class ZWavePanel : IDevicePanel
{
    /// <inheritdoc/>
    public string Id => PresenterName;

    /// <inheritdoc/>
    public string MenuTitle => "_Z-Wave Controller...";

    /// <inheritdoc/>
    public string PresenterName => ZWaveGlobals.ProviderKey;

    /// <inheritdoc/>
    public bool IsAvailable(string transport, int vendorId, int productId) =>
        string.Equals(transport, "serial", StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc/>
    public UiDefinition BuildDefinition() => ZWaveUiDefinition.Build();

    /// <inheritdoc/>
    public IControlSurface CreateSurface(DeviceSend send, IDeviceDecoder decoder) => new ZWaveControlSurface(send, decoder: decoder as ZWaveDecoder);

    /// <inheritdoc/>
    public IDeviceDecoder CreateDecoder() => new ZWaveDecoder();
}
