using System;

namespace OoBDev.System.Input;

/// <summary>
/// Implements an ICommand using delegate callbacks for execution logic and execution state checking.
/// </summary>
/// <param name="execute">The action to execute when the command is invoked.</param>
/// <param name="canExecute">Optional predicate to determine whether the command can execute. If null, the command can always execute.</param>
public class DelegateCommand(Action<object?> execute, Predicate<object?>? canExecute = default) : CommandBase
{
    /// <summary>
    /// Determines whether the command can execute in its current state by invoking the canExecute predicate.
    /// </summary>
    /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to null.</param>
    /// <returns>true if the canExecute predicate returns true or if no predicate was provided; otherwise, false.</returns>
    public override bool CanExecute(object? parameter) =>
        canExecute?.Invoke(parameter) ?? true;

    /// <summary>
    /// Executes the command by invoking the execute action delegate.
    /// </summary>
    /// <param name="parameter">Data used by the command. If the command does not require data to be passed, this object can be set to null.</param>
    public override void Execute(object? parameter) =>
        execute(parameter);
}
