using System.Windows.Input;
using OoBDev.UiDefinitions.Forms;

namespace OoBDev.UiDefinitions.ViewModels;

/// <summary>A button row that runs a command property of the model.</summary>
public sealed class ButtonViewModel : ControlViewModel
{
    /// <summary>Creates the view model and reads the model.</summary>
    /// <param name="control">The button.</param>
    /// <param name="binding">The bound model.</param>
    public ButtonViewModel(ButtonControl control, FormBinding binding)
        : base(control, binding)
    {
        Button = control;
        Invoke = new InvokeCommand(binding, control);
        Refresh();
    }

    /// <summary>The button definition.</summary>
    public ButtonControl Button { get; }

    /// <summary>Runs the model command.</summary>
    public ICommand Invoke { get; }

    /// <summary>The question a view asks before running the command, or null.</summary>
    public string? ConfirmMessage => Button.ConfirmMessage;

    /// <inheritdoc />
    protected override void RefreshValue() => ((InvokeCommand)Invoke).RaiseCanExecuteChanged();

    /// <inheritdoc />
    protected override bool IsLocked() => false;
}
