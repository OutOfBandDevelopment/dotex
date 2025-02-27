using System;

namespace OoBDev.System.Input;

public abstract class CommandBase : ICommand
{
    public virtual bool CanExecute(object? parameter) => true;

    public event EventHandler? CanExecuteChanged;

    public virtual void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, new EventArgs());

    public abstract void Execute(object? parameter);
}
