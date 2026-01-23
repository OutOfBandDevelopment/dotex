using System;

namespace OoBDev.System.Input;

/// <summary>
/// Provides a base implementation of the ICommand interface with support for command execution and change notification.
/// </summary>
public abstract class CommandBase : ICommand
{
    /// <summary>
    /// Determines whether the command can execute in its current state.
    /// </summary>
    /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to null.</param>
    /// <returns>true if this command can be executed; otherwise, false. The default implementation always returns true.</returns>
    public virtual bool CanExecute(object? parameter) => true;

    /// <summary>
    /// Occurs when changes occur that affect whether or not the command should execute.
    /// </summary>
    public event EventHandler? CanExecuteChanged;

    /// <summary>
    /// Raises the CanExecuteChanged event to notify subscribers that the command's execution status may have changed.
    /// </summary>
    public virtual void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, new EventArgs());

    /// <summary>
    /// Defines the method to be called when the command is invoked.
    /// </summary>
    /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to null.</param>
    public abstract void Execute(object? parameter);
}
