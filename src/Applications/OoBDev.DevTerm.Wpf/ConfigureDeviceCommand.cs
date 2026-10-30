using System;
using System.Threading.Tasks;
using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.DevTerm.Wpf;

/// <summary>A device configuration action that awaits the device without blocking the window.</summary>
/// <param name="canExecute">Whether the action applies now.</param>
/// <param name="run">The action; it reports its own failures.</param>
internal sealed class ConfigureDeviceCommand(Func<bool> canExecute, Func<Task> run) : CommandBase
{
    public override bool CanExecute(object? parameter) => canExecute();

    public override void Execute(object? parameter) => _ = run();
}
