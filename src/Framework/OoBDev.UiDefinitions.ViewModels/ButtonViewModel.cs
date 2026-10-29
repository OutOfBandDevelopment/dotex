using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using OoBDev.UiDefinitions.Forms;

namespace OoBDev.UiDefinitions.ViewModels;

/// <summary>A button row that runs a command property of the model.</summary>
public sealed class ButtonViewModel : ControlViewModel
{
    /// <summary>Creates the view model and reads the model.</summary>
    /// <param name="control">The button.</param>
    /// <param name="binding">The bound model.</param>
    /// <param name="colorPicker">The dialog for a colour picker button, or null.</param>
    /// <param name="siblings">The controls the colour picker may write to.</param>
    public ButtonViewModel(ButtonControl control, FormBinding binding, IColorPicker? colorPicker = null, IReadOnlyCollection<UiControl>? siblings = null)
        : base(control, binding)
    {
        Button = control;
        var target = colorPicker is not null && control.ColorPickerTargetCommandId is { } id ? siblings?.FirstOrDefault(c => c.Id == id) : null;
        Invoke = target is null ? new InvokeCommand(binding, control) : new PickColorCommand(binding, target, colorPicker!);
        Refresh();
    }

    /// <summary>The button definition.</summary>
    public ButtonControl Button { get; }

    /// <summary>Runs the model command.</summary>
    public ICommand Invoke { get; }

    /// <summary>The question a view asks before running the command, or null.</summary>
    public string? ConfirmMessage => Button.ConfirmMessage;

    /// <inheritdoc />
    protected override void RefreshValue() => ((CommandBase)Invoke).RaiseCanExecuteChanged();

    /// <inheritdoc />
    protected override bool IsLocked() => false;
}
