using System;
using System.Collections.Generic;
using OoBDev.UiDefinitions.Forms;

namespace OoBDev.UiDefinitions.ViewModels;

/// <summary>Creates the view model for a control definition.</summary>
public static class ControlViewModelFactory
{
    /// <summary>Creates the view model for <paramref name="control"/>.</summary>
    /// <param name="control">The control definition.</param>
    /// <param name="binding">The bound model.</param>
    /// <param name="colorPicker">The dialog a colour picker button opens; without it such a button runs its own command.</param>
    /// <param name="siblings">The controls a colour picker button may target.</param>
    /// <exception cref="NotSupportedException">The control kind is a live display control, not a form field.</exception>
    public static ControlViewModel Create(UiControl control, FormBinding binding, IColorPicker? colorPicker = null, IReadOnlyCollection<UiControl>? siblings = null)
    {
        ArgumentNullException.ThrowIfNull(control);
        return control switch
        {
            ToggleControl toggle => new ToggleViewModel(toggle, binding),
            ChoiceControl choice => new ChoiceViewModel(choice, binding),
            ButtonControl button => new ButtonViewModel(button, binding, colorPicker, siblings),
            TextFieldControl or NumericControl or SliderControl => new TextViewModel(control, binding),
            _ => throw new NotSupportedException($"'{control.GetType().Name}' has no form view model."),
        };
    }
}
