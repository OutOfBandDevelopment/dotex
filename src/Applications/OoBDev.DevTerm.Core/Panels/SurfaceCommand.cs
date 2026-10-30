using System;
using System.Windows.Input;
using OoBDev.UiDefinitions;

namespace OoBDev.DevTerm.Panels;

/// <summary>A panel button: sends its command id and the parameter text to the surface.</summary>
/// <param name="surface">The surface.</param>
/// <param name="commandId">The command id.</param>
/// <param name="onError">Receives a failure of the send.</param>
internal sealed class SurfaceCommand(IControlSurface surface, string commandId, Action<Exception>? onError) : ICommand
{
    public event EventHandler? CanExecuteChanged
    {
        add { }
        remove { }
    }

    public bool CanExecute(object? parameter) => true;

    public void Execute(object? parameter) => SurfaceSender.Send(surface, commandId, parameter as string, onError);
}
