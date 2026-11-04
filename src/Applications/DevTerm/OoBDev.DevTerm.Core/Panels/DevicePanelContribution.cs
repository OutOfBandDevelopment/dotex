using System;
using OoBDev.UiDefinitions;

namespace OoBDev.DevTerm.Panels;

/// <summary>Adapts a device library's <see cref="IDevicePanel"/> to the front ends' <see cref="IDevicePanelContribution"/>, sending through the session.</summary>
/// <param name="panel">The device panel.</param>
public sealed class DevicePanelContribution(IDevicePanel panel) : IDevicePanelContribution
{
    /// <inheritdoc />
    public string Id => panel.Id;

    /// <inheritdoc />
    public string MenuTitle => panel.MenuTitle;

    /// <inheritdoc />
    public string? PresenterName => panel.PresenterName;

    /// <inheritdoc />
    public bool IsAvailable(string transport, int vendorId, int productId) => panel.IsAvailable(transport, vendorId, productId);

    /// <inheritdoc />
    public UiDefinition BuildDefinition() => panel.BuildDefinition();

    /// <inheritdoc />
    public IControlSurface CreateSurface(Session session)
    {
        ArgumentNullException.ThrowIfNull(session);
        return panel.CreateSurface((data, cancellationToken) => session.SendAsync(data, cancellationToken));
    }
}
