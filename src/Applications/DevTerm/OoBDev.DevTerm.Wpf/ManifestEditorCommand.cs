using System;
using OoBDev.UiDefinitions.ViewModels;

namespace OoBDev.DevTerm.Wpf;

/// <summary>A manifest editor action that applies only in some states.</summary>
/// <param name="canExecute">Whether the action applies now.</param>
/// <param name="run">The action.</param>
internal sealed class ManifestEditorCommand(Func<bool> canExecute, Action run) : CommandBase
{
    public override bool CanExecute(object? parameter) => canExecute();

    public override void Execute(object? parameter) => run();

    internal void Refresh() => RaiseCanExecuteChanged();
}
