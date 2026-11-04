using System;
using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.DevTerm.Wpf;

/// <summary>An action of the theme windows.</summary>
/// <param name="canExecute">Whether the action applies now.</param>
/// <param name="execute">The action.</param>
internal sealed class ThemeBuilderCommand(Func<bool> canExecute, Action execute) : CommandBase
{
    public override bool CanExecute(object? parameter) => canExecute();

    public override void Execute(object? parameter) => execute();
}
