using System;
using OoBDev.UiDefinitions;

namespace OoBDev.DevTerm.Devices.Demo;

/// <summary>The demo panel as an <see cref="IDevicePanelContribution"/>, available on the loopback transport only.</summary>
public sealed class DemoPanelContribution : IDevicePanelContribution
{
    /// <inheritdoc />
    public string Id => "demo";

    /// <inheritdoc />
    public string MenuTitle => "_Demo Device Panel...";

    /// <inheritdoc />
    public string? PresenterName => null;

    /// <inheritdoc />
    public bool IsAvailable(string transport, int vendorId, int productId) =>
        string.Equals(transport, "loopback", StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc />
    public UiDefinition BuildDefinition() => DemoUiDefinition.Build();

    /// <inheritdoc />
    public IControlSurface CreateSurface(Session session, IStructuredPresenter? source = null) => new DemoControlSurface(session);
}
