using System.Windows.Input;
using System;
using OoBDev.UiDefinitions.Forms;

namespace OoBDev.UiDefinitions.ViewModels;

/// <summary>Runs the model's command property named by a button (<see cref="ButtonControl.CommandId"/>, or the button's own id).</summary>
/// <param name="binding">The bound model.</param>
/// <param name="control">The button.</param>
public sealed class InvokeCommand(FormBinding binding, ButtonControl control) : CommandBase
{
    private readonly FormBinding _binding = binding ?? throw new ArgumentNullException(nameof(binding));
    private readonly ButtonControl _control = control ?? throw new ArgumentNullException(nameof(control));

    /// <inheritdoc />
    public override bool CanExecute(object? parameter) =>
        _binding.GetValue(_control.CommandId ?? _control.Id) is ICommand command && command.CanExecute(null);

    /// <inheritdoc />
    public override void Execute(object? parameter) => _binding.Invoke(_control.CommandId ?? _control.Id);
}
