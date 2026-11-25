using System;
using OoBDev.UiDefinitions;

namespace OoBDev.Midi.Panels;

/// <summary>A MIDI port as an <see cref="IDevicePanel"/>. A MIDI line has no USB identity of its own, so it is offered on every serial connection.</summary>
public sealed class MidiPanel : IDevicePanel
{
    /// <inheritdoc />
    public string Id => "midi";

    /// <inheritdoc />
    public string MenuTitle => "_MIDI...";

    /// <inheritdoc />
    public string PresenterName => "midi";

    /// <inheritdoc />
    public bool IsAvailable(string transport, int vendorId, int productId) =>
        string.Equals(transport, "serial", StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public UiDefinition BuildDefinition() => MidiUiDefinition.Build();

    /// <inheritdoc />
    public IControlSurface CreateSurface(DeviceSend send, IDeviceDecoder decoder) => new MidiControlSurface(send);

    /// <inheritdoc />
    public IDeviceDecoder CreateDecoder() => new MidiDecoder();
}
