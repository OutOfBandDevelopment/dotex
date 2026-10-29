using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace OoBDev.UiDefinitions;

/// <summary>A <see cref="StripChartControl"/>'s rolling history per channel.</summary>
public sealed class StripChartState : LiveDisplayState
{
    private readonly Dictionary<string, Queue<double>> _history = new(StringComparer.Ordinal);
    private readonly Dictionary<string, double> _values = new(StringComparer.Ordinal);
    private readonly Dictionary<string, Expression?> _expressionByChannel = new(StringComparer.Ordinal);
    private readonly HashSet<string> _touchedExpressionChannelsThisBatch = new(StringComparer.Ordinal);

    public StripChartState(StripChartControl control)
    {
        ArgumentNullException.ThrowIfNull(control);
        Control = control;

        var ids = new List<string>();
        foreach (var channel in control.Channels)
        {
            _history.TryAdd(channel.Id, new Queue<double>());
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

    public StripChartControl Control { get; }

    protected override bool ReadsText => _expressionByChannel.Values.Any(e => e is not null);

    public override IReadOnlyList<string> ValueIds { get; }

    /// <summary>
    /// Hard ceiling on <see cref="Capacity"/>, regardless of what a manifest's
    /// <see cref="StripChartControl.HistoryLength"/> declares — protects against an
    /// unbounded-memory manifest (a per-sample <c>double</c> queue with no upper bound). See
    /// docs/bugs/resolved/050-strip-chart-history-unbounded.md.
    /// </summary>
    public const int MaxCapacity = 10_000;

    /// <summary>At least one sample is kept, and at most <see cref="MaxCapacity"/>, whatever the definition says.</summary>
    public int Capacity => Math.Clamp(Control.HistoryLength, 1, MaxCapacity);

    /// <summary>The channel's samples, oldest first (at most <see cref="Capacity"/>).</summary>
    public IReadOnlyList<double> SamplesOf(string channelId) =>
        _history.TryGetValue(channelId, out var samples) ? [.. samples] : [];

    /// <summary>
    /// The value axis: the declared [Minimum, Maximum] when both are set, otherwise the visible
    /// history's own span (widened to at least ±0.5 around a flat line, and to [0, 1] with no data)
    /// — a declared bound still wins for its own end.
    /// </summary>
    public (double Minimum, double Maximum) Scale()
    {
        if (Control.Minimum is { } fixedMin && Control.Maximum is { } fixedMax && fixedMax > fixedMin)
        {
            return (fixedMin, fixedMax);
        }

        var all = _history.Values.SelectMany(q => q).ToList();
        if (all.Count == 0)
        {
            return (Control.Minimum ?? 0, Control.Maximum ?? 1);
        }

        var min = Control.Minimum ?? all.Min();
        var max = Control.Maximum ?? all.Max();
        if (max - min < 1e-9)
        {
            min -= 0.5;
            max += 0.5;
        }

        return (min, max);
    }

    protected override void OnBatchStarting() => _touchedExpressionChannelsThisBatch.Clear();

    /// <summary>
    /// A plain channel enqueues one sample per raw arrival, exactly as before. An expression
    /// channel instead just notes it was touched — several raw ids it references can arrive in the
    /// same batch, and it must contribute exactly one derived sample for that batch, not one per raw
    /// id — the actual enqueue happens once in <see cref="OnBatchCompleted"/>, after every raw id in
    /// the batch has landed in <see cref="_values"/>.
    /// </summary>
    protected override bool ApplyNumber(string id, double value)
    {
        _values[id] = value;
        var changed = false;
        foreach (var (channelId, expression) in _expressionByChannel)
        {
            if (expression is null)
            {
                if (channelId == id)
                {
                    Enqueue(channelId, value);
                    changed = true;
                }
            }
            else if (expression.ReferencedIds.Contains(id, StringComparer.Ordinal))
            {
                _touchedExpressionChannelsThisBatch.Add(channelId);
            }
        }

        return changed;
    }

    /// <summary>A text-only change still re-evaluates the expression channels that read it, adding one derived sample for the batch.</summary>
    protected override void OnTextChanged(string id)
    {
        foreach (var (channelId, expression) in _expressionByChannel)
        {
            if (expression is not null && expression.ReferencedIds.Contains(id, StringComparer.Ordinal))
            {
                _touchedExpressionChannelsThisBatch.Add(channelId);
            }
        }
    }

    protected override bool OnBatchCompleted(bool changed)
    {
        foreach (var channelId in _touchedExpressionChannelsThisBatch)
        {
            Enqueue(channelId, _expressionByChannel[channelId]!.Evaluate(_values, Texts));
            changed = true;
        }

        return changed;
    }

    private void Enqueue(string channelId, double value)
    {
        var samples = _history[channelId];
        samples.Enqueue(value);
        while (samples.Count > Capacity)
        {
            samples.Dequeue();
        }
    }
}
