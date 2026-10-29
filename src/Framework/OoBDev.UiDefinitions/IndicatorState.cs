using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace OoBDev.UiDefinitions;

/// <summary>
/// An <see cref="IndicatorControl"/> whose <see cref="IndicatorControl.Expression"/> is set: derives
/// its displayed text from the live published values instead of showing its own raw id's text
/// verbatim. Only constructed by <see cref="LiveDisplayState.For"/> when the expression is set and
/// parses; an <see cref="IndicatorControl"/> with no expression never needs this class at all, so the
/// existing direct "set the label from the raw published id" path stays exactly as it was.
/// </summary>
public sealed class IndicatorState : LiveDisplayState
{
    private readonly Dictionary<string, double> _values = new(StringComparer.Ordinal);
    private readonly Expression? _expression;

    /// <summary>
    /// Never throws, even when <see cref="IndicatorControl.Expression"/> fails to parse — a bad
    /// expression leaves this state with no <see cref="ValueIds"/> and an always-null
    /// <see cref="Text"/> rather than taking down the control panel that built it. The load-time
    /// manifest validator is what actually catches a bad expression as an error; this is just the
    /// runtime fallback for one that somehow got through anyway.
    /// </summary>
    public IndicatorState(IndicatorControl control)
    {
        ArgumentNullException.ThrowIfNull(control);
        Control = control;
        _ = Expression.TryParse(control.Expression ?? string.Empty, out _expression, out _);
        ValueIds = _expression?.ReferencedIds ?? [];
    }

    public IndicatorControl Control { get; }

    protected override bool ReadsText => true;

    public override IReadOnlyList<string> ValueIds { get; }

    /// <summary>The expression evaluated against every value received so far, or null before any referenced id has arrived (or the expression failed to parse).</summary>
    public string? Text => _expression is not null && ValueIds.Any(id => _values.ContainsKey(id) || Texts.ContainsKey(id))
        ? Control.Display(_expression.EvaluateToText(_values, Texts))
        : null;

    protected override bool ApplyNumber(string id, double value)
    {
        _values[id] = value;
        return true;
    }
}
