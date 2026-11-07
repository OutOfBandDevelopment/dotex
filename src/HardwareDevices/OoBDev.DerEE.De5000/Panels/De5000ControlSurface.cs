using System;
using System.Threading;
using System.Threading.Tasks;
using OoBDev.UiDefinitions;

namespace OoBDev.DerEE.De5000.Panels;

/// <summary>The DE-5000 has no command side, so the surface accepts none.</summary>
public sealed class De5000ControlSurface : IControlSurface
{
    /// <inheritdoc />
    public Task InvokeAsync(string commandId, string? value, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(commandId);
        throw new ArgumentException($"The DE-5000 takes no commands ('{commandId}').", nameof(commandId));
    }
}
