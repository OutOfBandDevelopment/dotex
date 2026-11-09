using System;
using OoBDev.UiDefinitions;

namespace OoBDev.Fencing.ScoreMachines.Panels;

/// <summary>A fencing score machine (Favero or Saint George) as an <see cref="IDevicePanel"/>. The machines have no USB identity (they sit behind a serial adapter), so a panel is offered on every serial connection and the presenter name picks the family.</summary>
public abstract class FencingPanel : IDevicePanel
{
    private readonly MachineType _type;

    /// <summary>Creates the panel for a machine family (the DI-resolvable <see cref="FaveroPanel"/> and <see cref="SaintGeorgePanel"/> pass theirs).</summary>
    /// <param name="type">The machine family.</param>
    protected FencingPanel(MachineType type) => _type = type;

    /// <inheritdoc />
    public string Id => PresenterName;

    /// <inheritdoc />
    public string MenuTitle => _type == MachineType.Favero ? "_Favero Score Machine..." : "_Saint George Score Machine...";

    /// <inheritdoc />
    public string PresenterName => _type == MachineType.Favero ? FencingGlobals.FaveroKey : FencingGlobals.SaintGeorgeKey;

    /// <inheritdoc />
    public bool IsAvailable(string transport, int vendorId, int productId) =>
        string.Equals(transport, "serial", StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public UiDefinition BuildDefinition() => FencingUiDefinition.Build(_type == MachineType.Favero ? "Favero" : "Saint George");

    /// <inheritdoc />
    public IControlSurface CreateSurface(DeviceSend send) => new FencingControlSurface();

    /// <inheritdoc />
    public IDeviceDecoder CreateDecoder() => new ScoreMachineDecoder(_type);
}
