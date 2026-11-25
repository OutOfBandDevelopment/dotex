using OoBDev.UiDefinitions;
using OoBDev.WebApi.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace OoBDev.WebApi.Services;

/// <summary>Evaluates a UI definition for a set of values: which controls are visible and enabled, and whether each value is valid.</summary>
public class UiDefinitionEvaluator
{
    /// <summary>Evaluates every control of the definition.</summary>
    /// <param name="definition">The definition.</param>
    /// <param name="values">The values entered so far, by control id; missing ones fall back to the control default.</param>
    /// <returns>One state per control, in definition order.</returns>
    public virtual IReadOnlyList<UiControlStateModel> Evaluate(UiDefinition definition, IReadOnlyDictionary<string, string> values)
    {
        ArgumentNullException.ThrowIfNull(definition);
        ArgumentNullException.ThrowIfNull(values);

        var effective = definition.Sections
            .SelectMany(s => s.Controls)
            .ToDictionary(c => c.Id, c => values.TryGetValue(c.Id, out var v) ? v : DefaultValue(c), StringComparer.Ordinal);

        var states = new List<UiControlStateModel>();
        foreach (var section in definition.Sections)
        {
            var sectionVisible = IsMet(section.VisibleWhen, effective);
            foreach (var control in section.Controls)
            {
                var validation = Validate(control, effective[control.Id]);
                var enabled = IsMet(control.EnabledWhen, effective);
                states.Add(new UiControlStateModel
                {
                    Id = control.Id,
                    Visible = sectionVisible && IsMet(control.VisibleWhen, effective),
                    Enabled = enabled,
                    DisabledReason = enabled ? null : control.DisabledReason,
                    Valid = validation.IsValid,
                    Value = validation.Value,
                    Error = validation.Error,
                });
            }
        }

        return states;
    }

    /// <summary>The value a control shows before the user touches it.</summary>
    /// <param name="control">The control.</param>
    /// <returns>The default as text.</returns>
    public static string DefaultValue(UiControl control)
    {
        ArgumentNullException.ThrowIfNull(control);
        return control switch
        {
            ToggleControl t => t.DefaultValue ? "true" : "false",
            NumericControl n => n.DefaultValue.ToString(CultureInfo.InvariantCulture),
            SliderControl s => s.DefaultValue.ToString(CultureInfo.InvariantCulture),
            ChoiceControl c => c.DefaultValue ?? c.Options.FirstOrDefault() ?? string.Empty,
            TextFieldControl t => t.DefaultValue ?? string.Empty,
            IndicatorControl i => i.DefaultValue ?? string.Empty,
            _ => string.Empty,
        };
    }

    private static bool IsMet(UiCondition? condition, Dictionary<string, string> effective) =>
        condition is null || condition.IsMetBy(effective.GetValueOrDefault(condition.Id));

    private static ValueValidationResult Validate(UiControl control, string value) => control switch
    {
        NumericControl n => ValueValidator.Validate(new ValueConstraint { Kind = ValueKind.Number, Minimum = n.Minimum, Maximum = n.Maximum }, value),
        SliderControl s => ValueValidator.Validate(new ValueConstraint { Kind = ValueKind.Number, Minimum = s.Minimum, Maximum = s.Maximum }, value),
        TextFieldControl { MaxLength: { } max } when value.Length > max => ValueValidationResult.Invalid(value, $"At most {max} characters."),
        TextFieldControl t => ValueValidator.Validate(t.Constraint, value),
        ChoiceControl c when value.Length > 0 && !value.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).All(c.Options.Contains) =>
            ValueValidationResult.Invalid(value, "Not one of the options."),
        _ => ValueValidationResult.Valid(value),
    };
}
