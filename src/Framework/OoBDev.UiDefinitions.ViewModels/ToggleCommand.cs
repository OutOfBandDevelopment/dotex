using System;
using OoBDev.UiDefinitions.Forms;

namespace OoBDev.UiDefinitions.ViewModels;

/// <summary>Writes a toggle's value: the parameter is a <see cref="bool"/>, or absent to flip the current value.</summary>
/// <param name="binding">The bound model.</param>
/// <param name="control">The toggle.</param>
public sealed class ToggleCommand(FormBinding binding, ToggleControl control) : CommandBase
{
    private readonly FormBinding _binding = binding ?? throw new ArgumentNullException(nameof(binding));
    private readonly ToggleControl _control = control ?? throw new ArgumentNullException(nameof(control));

    /// <inheritdoc />
    public override bool CanExecute(object? parameter) => !_binding.IsReadOnly(_control.Id);

    /// <inheritdoc />
    public override void Execute(object? parameter) =>
        _binding.SetBool(_control.Id, parameter is bool value ? value : !_binding.GetBool(_control.Id));
}
