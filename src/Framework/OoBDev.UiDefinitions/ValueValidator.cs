using System;
using System.Globalization;

namespace OoBDev.UiDefinitions;

/// <summary>
/// The one value validator both control-panel renderers (TUI <c>ControlPanelMode</c>, WPF
/// <c>ControlPanelWindow</c>) run on commit, before anything is sent: an input that fails its
/// <see cref="ValueConstraint"/> is rejected with a message; a valid number is normalized to an
/// invariant-culture string (<c>" 1e3 "</c> → <c>"1000"</c>, <c>"05"</c> → <c>"5"</c>).
/// </summary>
public static class ValueValidator
{
    /// <summary>Validates <paramref name="input"/> against <paramref name="constraint"/>; no constraint (or <see cref="ValueKind.Text"/>) accepts anything unchanged.</summary>
    public static ValueValidationResult Validate(ValueConstraint? constraint, string? input)
    {
        var text = input ?? string.Empty;
        if (constraint is null || constraint.Kind == ValueKind.Text)
        {
            return ValueValidationResult.Valid(text);
        }

        var trimmed = text.Trim();
        if (!double.TryParse(trimmed, NumberStyles.Float, CultureInfo.InvariantCulture, out var number)
            || double.IsNaN(number)
            || double.IsInfinity(number))
        {
            return ValueValidationResult.Invalid(
                text,
                constraint.Kind == ValueKind.Integer ? $"'{trimmed}' is not a whole number." : $"'{trimmed}' is not a number.");
        }

        if (constraint.Kind == ValueKind.Integer && Math.Floor(number) != number)
        {
            return ValueValidationResult.Invalid(text, $"'{trimmed}' is not a whole number.");
        }

        var outOfRange = (constraint.Minimum is { } min && number < min) || (constraint.Maximum is { } max && number > max);
        if (outOfRange)
        {
            if (!constraint.ClampToRange)
            {
                return ValueValidationResult.Invalid(text, $"{trimmed} is out of range ({constraint.DescribeRange()}).");
            }

            number = Math.Clamp(number, constraint.Minimum ?? double.MinValue, constraint.Maximum ?? double.MaxValue);
        }

        return ValueValidationResult.Valid(Normalize(number, constraint.Kind));
    }

    /// <summary>
    /// The constraint a renderer checks a value-holding control's committed input against:
    /// <see cref="TextFieldControl.Constraint"/> as declared, or the clamped numeric range a
    /// <see cref="SliderControl"/>/<see cref="NumericControl"/> implies. Null for controls whose value
    /// can't be typed freely (toggle, choice, button, indicator).
    /// </summary>
    public static ValueConstraint? ConstraintFor(UiControl control) => control switch
    {
        TextFieldControl textField => textField.Constraint,
        NumericControl numeric => ValueConstraint.ForRange(numeric.Minimum, numeric.Maximum),
        SliderControl slider => ValueConstraint.ForRange(slider.Minimum, slider.Maximum),
        _ => null,
    };

    private static string Normalize(double number, ValueKind kind) =>
        kind == ValueKind.Integer
            ? number.ToString("0", CultureInfo.InvariantCulture)
            : number.ToString(CultureInfo.InvariantCulture);
}
