using System;
using System.Linq;
using OoBDev.UiDefinitions.Forms;

namespace OoBDev.UiDefinitions.ViewModels;

/// <summary>Checks or unchecks one option of a check list; the parameter is the option text.</summary>
/// <param name="binding">The bound model.</param>
/// <param name="control">The check list.</param>
public sealed class ToggleOptionCommand(FormBinding binding, ChoiceControl control) : CommandBase
{
    private readonly FormBinding _binding = binding ?? throw new ArgumentNullException(nameof(binding));
    private readonly ChoiceControl _control = control ?? throw new ArgumentNullException(nameof(control));

    /// <inheritdoc />
    public override bool CanExecute(object? parameter) =>
        parameter is string option && !_binding.IsReadOnly(_control.Id) && _control.Options.Contains(option, StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc />
    public override void Execute(object? parameter)
    {
        if (!CanExecute(parameter))
        {
            return;
        }

        var option = (string)parameter!;
        var selected = _binding.GetSelection(_control.Id).ToList();
        if (selected.RemoveAll(s => string.Equals(s, option, StringComparison.OrdinalIgnoreCase)) > 0)
        {
            _binding.SetSelection(_control.Id, selected);
            return;
        }

        selected.Add(option);
        _binding.SetSelection(_control.Id, _control.Options.Where(o => selected.Contains(o, StringComparer.OrdinalIgnoreCase)));
    }
}
