using System;
using System.Windows.Input;

namespace OoBDev.UiDefinitions.ViewModels;

/// <summary>Base for the panel commands: each is a real type with its dependencies injected, so it can be tested alone.</summary>
public abstract class CommandBase : ICommand
{
    /// <inheritdoc />
    public event EventHandler? CanExecuteChanged;

    /// <inheritdoc />
    public virtual bool CanExecute(object? parameter) => true;

    /// <inheritdoc />
    public abstract void Execute(object? parameter);

    /// <summary>Tells the view to ask <see cref="CanExecute"/> again.</summary>
    public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
}
