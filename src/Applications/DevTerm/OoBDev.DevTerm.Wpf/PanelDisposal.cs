using System;

namespace OoBDev.DevTerm.Wpf;

/// <summary>Disposes several parts of one open panel together.</summary>
/// <param name="parts">The parts, disposed in order.</param>
internal sealed class PanelDisposal(params IDisposable[] parts) : IDisposable
{
    public void Dispose()
    {
        foreach (var part in parts)
        {
            part.Dispose();
        }
    }
}
