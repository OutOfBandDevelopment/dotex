using System;
using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.DevTerm.Wpf;

/// <summary>Runs a device scan.</summary>
/// <param name="scan">The scan.</param>
internal sealed class ScanDevicesCommand(Action scan) : CommandBase
{
    public override void Execute(object? parameter) => scan();
}
