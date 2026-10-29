using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;

namespace OoBDev.UiDefinitions;

/// <summary>
/// The framework-agnostic live state behind one display control (<see cref="BarGraphControl"/>,
/// <see cref="StripChartControl"/>, <see cref="VectorControl"/>): both renderers feed it the
/// structured presenter's published values and draw from it, so what's plotted — clamping,
/// history, scaling, coordinate conversion — is decided once, here, and tested without a UI.
/// </summary>
public abstract class LiveDisplayState
{
    /// <summary>The published value ids this display reads.</summary>
    public abstract IReadOnlyList<string> ValueIds { get; }

    /// <summary>The latest raw text published for each of <see cref="ValueIds"/>, so an expression can match or compare it as a string.</summary>
    protected Dictionary<string, string> Texts { get; } = new(StringComparer.Ordinal);

    /// <summary>Applies one published value; false when the id isn't one of <see cref="ValueIds"/> or the text holds no number (nothing changes).</summary>
    public bool Apply(string id, string text) => ApplyAll([new KeyValuePair<string, string>(id, text)]);

    /// <summary>
    /// Applies one <c>ValuesChanged</c> batch as a single update (a vector moves once for an x+y
    /// pair published together, not once per coordinate); true when anything changed.
    /// </summary>
    public bool ApplyAll(IEnumerable<KeyValuePair<string, string>> values)
    {
        ArgumentNullException.ThrowIfNull(values);

        OnBatchStarting();
        var changed = false;
        foreach (var (id, text) in values)
        {
            if (!ValueIds.Contains(id, StringComparer.Ordinal))
            {
                continue;
            }

            var previousText = Texts.TryGetValue(id, out var old) ? old : null;
            if (ReadsText)
            {
                Texts[id] = text;
            }

            if (ChartValue.TryParse(text, out var value))
            {
                changed |= ApplyNumber(id, value);
            }
            else if (ReadsText)
            {
                // Text with no number in it (a model name) changes only what a string expression sees.
                if (!string.Equals(previousText, text, StringComparison.Ordinal))
                {
                    OnTextChanged(id);
                    changed = true;
                }
            }
        }

        return OnBatchCompleted(changed);
    }

    /// <summary>True when an expression of this display may read published text (<see cref="Texts"/>); a display of plain numeric channels ignores text with no number in it.</summary>
    protected virtual bool ReadsText => false;

    protected abstract bool ApplyNumber(string id, double value);

    /// <summary>Called when an id published text with no number in it and that text differs from before, so a display can re-derive anything that reads it.</summary>
    protected virtual void OnTextChanged(string id)
    {
    }

    protected virtual void OnBatchStarting()
    {
    }

    /// <summary>
    /// Runs after every raw id in the batch has reached <see cref="ApplyNumber"/> — lets a subclass
    /// report a change that didn't come from <see cref="ApplyNumber"/>'s own return value (e.g. an
    /// expression-backed <see cref="StripChartState"/> channel, which only knows it has a complete
    /// derived sample once the whole batch has landed). Returns the final "did anything change"
    /// verdict <see cref="ApplyAll"/> reports back to its caller; the default passes
    /// <paramref name="changed"/> through unchanged.
    /// </summary>
    protected virtual bool OnBatchCompleted(bool changed) => changed;

    /// <summary>The state for <paramref name="control"/>, or null when it isn't a live display control.</summary>
    public static LiveDisplayState? For(UiControl control) => control switch
    {
        BarGraphControl bar => new BarGraphState(bar),
        StripChartControl strip => new StripChartState(strip),
        VectorControl vector => new VectorState(vector),
        IndicatorControl { Expression: { Length: > 0 } } indicator => new IndicatorState(indicator),
        _ => null,
    };
}
