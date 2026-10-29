using System;
using OoBDev.UiDefinitions.Forms;

namespace OoBDev.UiDefinitions.ViewModels;

/// <summary>Asks <see cref="IColorPicker"/> for a colour and writes it as <c>r,g,b</c> text to the control named by <see cref="ButtonControl.ColorPickerTargetCommandId"/>.</summary>
/// <param name="binding">The bound model.</param>
/// <param name="target">The control that receives the colour.</param>
/// <param name="picker">The dialog.</param>
public sealed class PickColorCommand(FormBinding binding, UiControl target, IColorPicker picker) : CommandBase
{
    private readonly FormBinding _binding = binding ?? throw new ArgumentNullException(nameof(binding));
    private readonly UiControl _target = target ?? throw new ArgumentNullException(nameof(target));
    private readonly IColorPicker _picker = picker ?? throw new ArgumentNullException(nameof(picker));

    /// <summary>The last colour that was picked, or null.</summary>
    public string? LastColor { get; private set; }

    /// <inheritdoc />
    public override void Execute(object? parameter)
    {
        var picked = _picker.Pick(LastColor ?? _binding.GetText(_target.Id));
        if (picked is null)
        {
            return;
        }

        if (_binding.SetText(_target, picked).IsValid)
        {
            LastColor = picked;
        }
    }
}
