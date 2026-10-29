using System.Windows.Input;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using OoBDev.UiDefinitions.Forms;

namespace OoBDev.UiDefinitions.ViewModels;

/// <summary>Runs the model's command property named by a button (<see cref="ButtonControl.CommandId"/>, or the button's own id).</summary>
/// <param name="binding">The bound model.</param>
/// <param name="control">The button.</param>
/// <param name="siblings">The controls whose values a button with <see cref="ButtonControl.ParameterFieldIds"/> reads.</param>
public sealed class InvokeCommand(FormBinding binding, ButtonControl control, IReadOnlyCollection<UiControl>? siblings = null) : CommandBase
{
    private readonly FormBinding _binding = binding ?? throw new ArgumentNullException(nameof(binding));
    private readonly ButtonControl _control = control ?? throw new ArgumentNullException(nameof(control));

    /// <inheritdoc />
    public override bool CanExecute(object? parameter) =>
        _binding.GetValue(_control.CommandId ?? _control.Id) is ICommand command && command.CanExecute(Parameter());

    /// <inheritdoc />
    public override void Execute(object? parameter) => _binding.Invoke(_control.CommandId ?? _control.Id, Parameter());

    private string? Parameter()
    {
        if (_control.ParameterFieldIds is not { Count: > 0 } ids) return null;

        var known = siblings?.Select(c => c.Id).ToHashSet(StringComparer.Ordinal) ?? [];
        var text = known.ToDictionary(id => id, _binding.GetText, StringComparer.Ordinal);
        var numbers = text
            .Where(pair => double.TryParse(pair.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out _))
            .ToDictionary(pair => pair.Key, pair => double.Parse(pair.Value, CultureInfo.InvariantCulture), StringComparer.Ordinal);
        return string.Join(',', ids.Select((id, index) => Value(id, index, known, text, numbers)));
    }

    // A blank or unparsable expression keeps the bare field value, as ParameterExpressions documents.
    private string Value(string id, int index, HashSet<string> known, Dictionary<string, string> text, Dictionary<string, double> numbers)
    {
        if (!known.Contains(id)) return string.Empty;

        var source = _control.ParameterExpressions is { } list && index < list.Count ? list[index] : null;
        if (string.IsNullOrWhiteSpace(source) || !Expression.TryParse(source, out var expression, out _)) return text[id];

        try
        {
            return expression!.EvaluateToText(numbers, text);
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException or FormatException or KeyNotFoundException)
        {
            return text[id];
        }
    }
}
