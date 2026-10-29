using System.Linq;
using System;
using OoBDev.UiDefinitions.Forms;

namespace OoBDev.UiDefinitions.ViewModels;

/// <summary>Picks one option of a dropdown or radio choice; the parameter is the option text.</summary>
/// <param name="binding">The bound model.</param>
/// <param name="control">The choice.</param>
public sealed class SelectOptionCommand(FormBinding binding, ChoiceControl control) : CommandBase
{
    private readonly FormBinding _binding = binding ?? throw new ArgumentNullException(nameof(binding));
    private readonly ChoiceControl _control = control ?? throw new ArgumentNullException(nameof(control));

    /// <inheritdoc />
    public override bool CanExecute(object? parameter) =>
        parameter is string option && !_binding.IsReadOnly(_control.Id) && _control.Options.Contains(option, StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc />
    public override void Execute(object? parameter)
    {
        if (CanExecute(parameter))
        {
            _binding.SetText(_control, (string)parameter!);
        }
    }
}
