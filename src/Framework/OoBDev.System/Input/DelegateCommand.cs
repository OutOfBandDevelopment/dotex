﻿using System;

namespace OoBDev.System.Input;

public class DelegateCommand(Action<object?> execute, Predicate<object?>? canExecute = default) : CommandBase
{
    public override bool CanExecute(object? parameter) =>
        canExecute?.Invoke(parameter) ?? true;

    public override void Execute(object? parameter) =>
        execute(parameter);
}
