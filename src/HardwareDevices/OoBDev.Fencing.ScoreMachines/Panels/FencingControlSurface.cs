using System;
using System.Threading;
using System.Threading.Tasks;
using OoBDev.UiDefinitions;

namespace OoBDev.Fencing.ScoreMachines.Panels;

/// <summary>The score box takes no commands; every invocation is rejected.</summary>
public sealed class FencingControlSurface : IControlSurface
{
    /// <inheritdoc />
    public Task InvokeAsync(string commandId, string? value, CancellationToken cancellationToken) =>
        throw new ArgumentException($"The score machine takes no commands ({commandId}).", nameof(commandId));
}
