using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace OoBDev.UiDefinitions;

/// <summary>A <see cref="BarGraphControl"/>'s latest value per channel.</summary>
public sealed class BarGraphState : LiveDisplayState
{
    private readonly Dictionary<string, double> _values = new(StringComparer.Ordinal);
    private readonly Dictionary<string, Expression?> _expressionByChannel = new(StringComparer.Ordinal);

    public BarGraphState(BarGraphControl control)
    {
        ArgumentNullException.ThrowIfNull(control);
        Control = control;

        var ids = new List<string>();
        foreach (var channel in control.Channels)
        {
            if (channel.Expression is { Length: > 0 } text && Expression.TryParse(text, out var expr, out _))
            {
                _expressionByChannel[channel.Id] = expr;
                ids.AddRange(expr!.ReferencedIds);
            }
            else
            {
                _expressionByChannel[channel.Id] = null;
                ids.Add(channel.Id);
            }
        }

        ValueIds = [.. ids.Distinct(StringComparer.Ordinal)];
    }

    public BarGraphControl Control { get; }

    protected override bool ReadsText => _expressionByChannel.Values.Any(e => e is not null);

    public override IReadOnlyList<string> ValueIds { get; }

    /// <summary>
    /// The channel's latest value — either its own raw published value, or (when the channel
    /// declares an <see cref="ChartChannel.Expression"/>) that expression evaluated against every
    /// raw id it references — or null before any relevant raw value has arrived.
    /// </summary>
    public double? ValueOf(string channelId)
    {
        if (!_expressionByChannel.TryGetValue(channelId, out var expression))
        {
            return null;
        }

        if (expression is null)
        {
            return _values.TryGetValue(channelId, out var value) ? value : null;
        }

        return expression.ReferencedIds.Any(id => _values.ContainsKey(id) || Texts.ContainsKey(id)) ? expression.Evaluate(_values, Texts) : null;
    }

    /// <summary>How full the channel's bar is, in [0, 1] (0 before any value, and for an empty range).</summary>
    public double FractionOf(string channelId)
    {
        var range = Control.Maximum - Control.Minimum;
        if (ValueOf(channelId) is not { } value || range <= 0)
        {
            return 0;
        }

        return Math.Clamp((value - Control.Minimum) / range, 0, 1);
    }

    protected override bool ApplyNumber(string id, double value)
    {
        _values[id] = value;
        return true;
    }
}
