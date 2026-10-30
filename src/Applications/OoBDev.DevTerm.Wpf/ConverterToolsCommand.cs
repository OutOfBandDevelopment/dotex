using System;
using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.DevTerm.Wpf;

/// <summary>A list action of the converter tools window.</summary>
/// <param name="canExecute">Whether the action applies now.</param>
/// <param name="execute">The action.</param>
internal sealed class ConverterToolsCommand(Func<bool> canExecute, Action execute) : CommandBase
{
    public override bool CanExecute(object? parameter) => canExecute();

    public override void Execute(object? parameter) => execute();
}
